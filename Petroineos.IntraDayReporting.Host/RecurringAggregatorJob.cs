namespace Petroineos.IntraDayReporting.Host
{
    public class RecurringAggregatorJob : BackgroundService
    {
        private readonly ILogger<RecurringAggregatorJob> _logger;

        public RecurringAggregatorJob(ILogger<RecurringAggregatorJob> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}