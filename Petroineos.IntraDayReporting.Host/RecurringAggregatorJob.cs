using Petroineos.IntraDayReporting.Domain.Entities;
using Petroineos.IntraDayReporting.Domain.Infrastructure;
using Petroineos.IntraDayReporting.Domain.Interfaces;
using Polly.Retry;
using Polly;

namespace Petroineos.IntraDayReporting.Host
{
    public class RecurringAggregatorJob : BackgroundService
    {
        private const int ReportExecutionTimingThresholdSeconds = 60;
        public const int RetryAttempts = 3;
        private readonly ILogger<RecurringAggregatorJob> _logger;
        private int MaximumSecondsDelayBetweenConsecutiveReportExtractions { get; set; }
        private readonly ITradingService _tradingService;
        private readonly IClockService _clockService;
        private readonly ICsvGeneratorService _csvGeneratorService;
        private readonly IReportsRepo _reportsRepo;
        public RecurringAggregatorJob(ILogger<RecurringAggregatorJob> logger, JobConfig config, ITradingService tradingService, IClockService clockService, ICsvGeneratorService csvGeneratorService, IReportsRepo reportsRepo)
        {
            _logger = logger;
            MaximumSecondsDelayBetweenConsecutiveReportExtractions = config.MaximumSecondsDelayBetweenConsecutiveReportExtractions;
            _tradingService = tradingService;
            _clockService = clockService;
            _csvGeneratorService = csvGeneratorService;
            _reportsRepo = reportsRepo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pendingReports = await ExecuteEarliestReportGenerationAsync();
                    if (pendingReports == 0)
                    {
                        var delayMilliseconds = this.MaximumSecondsDelayBetweenConsecutiveReportExtractions * 1000;
                        _logger.LogInformation($"No pending report. Waiting for execution, {delayMilliseconds} ms");
                        await Task.Delay(delayMilliseconds, stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"{pendingReports} pending reports in the queue. Not going to wait ");
                        await Task.Delay(0, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // log the exception
                    _logger.LogError(ex, $"Exception while calling method {nameof(ExecuteEarliestReportGenerationAsync)}");
                }

            }
        }

        public async Task<int> ExecuteEarliestReportGenerationAsync()
        {
            var currentClockTime = _clockService.GetCurrentTime();
            var startOfReportingDay = GetStartOfReportingDay();
            var endOfReportingDay = GetEndOfReportingDay();

            _logger.LogInformation($"Executing job {currentClockTime}, startOfReportingDay={startOfReportingDay} , endOfReportingDay={endOfReportingDay}");

            var retryPolicy = CreateExponentialBackoffPolicy();

            //Get existing csv reports, if there are any
            var allExistingReports = await _reportsRepo.GetReports();

            var alreadyGeneratedReports = allExistingReports
                .Where(rpt => rpt.ReportDate > startOfReportingDay)
                .OrderBy(rpt => rpt.ReportDate)
                .ToList();

            _logger
                .LogInformation($"Found {alreadyGeneratedReports.Count} reports in the repository. Most recent being generated at '{alreadyGeneratedReports.LastOrDefault()?.ReportDate}'");

            List<DateTime> desiredReportExecutionTimings = GenerateTableOfDesiredReportExecutionTimings(startOfReportingDay, endOfReportingDay);

            var allReportsWhichHaveMissedOrNotYetGenerated = desiredReportExecutionTimings
                .SkipWhile(d => IsDesiredExecutionTimingAlreadyGenerated(d, alreadyGeneratedReports))
                .ToList();

            var allReportsWhichAreAlreadyDue = allReportsWhichHaveMissedOrNotYetGenerated
                .Where(d => d <= currentClockTime).ToList();

            if (allReportsWhichAreAlreadyDue.Count == 0)
            {
                _logger.LogInformation($"All reports generated for the given reporting date. Current time:{_clockService.GetCurrentTime()}");
                return 0;
            }

            _logger.LogInformation($"There are {allReportsWhichAreAlreadyDue.Count} reports which are already due for generation");
            var firstMissingExecutionTiming = allReportsWhichAreAlreadyDue.First();

            _logger.LogInformation($"Going to fetch all trades for the date: {firstMissingExecutionTiming}");

            var trades = await retryPolicy
                    .ExecuteAsync<List<TradePosition>>(() => _tradingService.GetTradesAsync(firstMissingExecutionTiming));


            _logger.LogInformation($"Found {trades.Count} trades for the date: {firstMissingExecutionTiming}");

            List<AggregatedTradePosition> aggregatedPositions = CalculateAggregatedTrades(trades);
            _logger.LogInformation($"The report will have {aggregatedPositions.Count} lines of data");
            string csvContents = CreateAggregatedCsvReport(aggregatedPositions);

            _logger.LogInformation($"The report generated at time {firstMissingExecutionTiming} will be saved");

            await retryPolicy.ExecuteAsync(() => _reportsRepo.SaveReport(firstMissingExecutionTiming, csvContents));

            return (allReportsWhichAreAlreadyDue.Count - 1);


        }

        /// <summary>
        /// Local start time of the day is 23:00 (11 pm) on the previous day.
        /// </summary>
        /// <returns></returns>
        public DateTime GetStartOfReportingDay()
        {
            var currentDateTime = _clockService.GetCurrentTime();
            return (currentDateTime.Hour >= 23) ? (currentDateTime.Date.AddHours(23)) : (currentDateTime.Date.AddDays(-1).AddHours(23));
        }

        public DateTime GetEndOfReportingDay()
        {
            return GetStartOfReportingDay().AddDays(+1).Date.AddHours(22).AddMinutes(59);
        }

        private string CreateAggregatedCsvReport(List<AggregatedTradePosition> trades)
        {
            return _csvGeneratorService.GenerateCsv(trades);
        }

        private List<DateTime> GenerateTableOfDesiredReportExecutionTimings(DateTime startOfReportingDay, DateTime endOfReportingDay)
        {
            _logger.LogInformation($"Generating table of expected report timings using the Max permitted delay between consecutive reports:{MaximumSecondsDelayBetweenConsecutiveReportExtractions} seconds");
            var desiredReportExecutionTimings = new List<DateTime>();

            while (true)
            {
                if (!desiredReportExecutionTimings.Any())
                {
                    desiredReportExecutionTimings.Add(startOfReportingDay.AddSeconds(MaximumSecondsDelayBetweenConsecutiveReportExtractions));
                }
                else
                {
                    desiredReportExecutionTimings.Add(desiredReportExecutionTimings.Last().AddSeconds(MaximumSecondsDelayBetweenConsecutiveReportExtractions));
                }

                if (desiredReportExecutionTimings.Last() >= endOfReportingDay)
                {
                    break;
                }
            }
            _logger.LogInformation($"Generated {desiredReportExecutionTimings.Count} execution timings");
            _logger.LogInformation($"First execution time in the current reporting day:{desiredReportExecutionTimings.First()}");
            _logger.LogInformation($"Last execution time in the current reporting day:{desiredReportExecutionTimings.Last()}");
            return desiredReportExecutionTimings;
        }

        /// <summary>
        /// Returns True, if there is already a report for the specified execution time.
        /// </summary>
        /// <param name="desiredExecutionTiming">Execution time </param>
        /// <param name="alreadyGeneratedReports">Reports from the reports repository</param>
        /// <returns>True/False</returns>
        private bool IsDesiredExecutionTimingAlreadyGenerated(
            DateTime desiredExecutionTiming,
            List<AggregatedReportBase> alreadyGeneratedReports)
        {
            if (!alreadyGeneratedReports.Any())
            {
                return false;
            }
            return alreadyGeneratedReports.Any(r => ApproximateDateTimeMatch(r.ReportDate, desiredExecutionTiming));
        }

        private bool ApproximateDateTimeMatch(DateTime dateTime1, DateTime dateTime2)
        {
            return Math.Abs((dateTime1 - dateTime2).TotalSeconds) < ReportExecutionTimingThresholdSeconds;
        }

        public List<AggregatedTradePosition> CalculateAggregatedTrades(List<TradePosition> trades)
        {
            var groupTradesByPeriod = trades.SelectMany(t => t.Periods).GroupBy(tp => tp.Period).ToList();
            var aggregatedPositions = groupTradesByPeriod.Select(g => new AggregatedTradePosition(g.Key, g.Sum(tp => tp.Volume)));
            return aggregatedPositions.OrderBy(ag => ag.Period).ToList();
        }

        /// <summary>
        /// Wait and retry 3 times in the event of an exception
        /// </summary>
        /// <returns></returns>
        private static AsyncRetryPolicy CreateExponentialBackoffPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    RetryAttempts,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }
}