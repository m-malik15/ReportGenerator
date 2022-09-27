using Microsoft.Extensions.Logging;
using Petroineos.IntraDayReporting.Domain.Entities;
using Petroineos.IntraDayReporting.Domain.Extensions;
using Petroineos.IntraDayReporting.Domain.Interfaces;

namespace Petroineos.IntraDayReporting.Repository
{
    public class NetworkFileShareReportsRepository : IReportsRepo
    {
        private readonly string _baseFolder;
        private readonly ILogger<NetworkFileShareReportsRepository> _logger;

        public NetworkFileShareReportsRepository(string reportsBaseFolder, ILogger<NetworkFileShareReportsRepository> logger)
        {
            if (!System.IO.Directory.Exists(reportsBaseFolder))
            {
                throw new InvalidOperationException($"The specified folder was not found: {reportsBaseFolder}");
            }
            this._baseFolder = reportsBaseFolder;
            this._logger = logger;
        }

        public Task<List<AggregatedReportBase>> GetReports()
        {
            var csvFiles = System.IO.Directory.GetFiles(_baseFolder, "*.csv");

            var results = new List<FileSystemAggregatedReportRepository>();

            csvFiles.ToList().ForEach(f =>
            {
                var date = System.IO.Path.GetFileName(f).ToDateTime();
                if (date != null)
                {
                    results.Add(new FileSystemAggregatedReportRepository(date.Value, date.Value.ConvertDateToPeriod(), f));
                }
            });

            _logger.LogInformation($"After querying folder {_baseFolder}. Found {csvFiles.Length} files");
            return Task.FromResult(results.OrderBy(rpt => rpt.ReportDate).Cast<AggregatedReportBase>().ToList());
        }

        public Task SaveReport(DateTime reportDateTime, string contents)
        {
            var reportFile = reportDateTime.ToReportFileName();
            var absolutePath = System.IO.Path.Combine(_baseFolder, reportFile);
            System.IO.File.WriteAllText(absolutePath, contents);
            _logger.LogInformation($"The report with date:{reportDateTime} was saved with the file:{reportFile}");
            return Task.CompletedTask;
        }
    }
}
