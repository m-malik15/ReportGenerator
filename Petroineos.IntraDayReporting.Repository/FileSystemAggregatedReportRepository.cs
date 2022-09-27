using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petroineos.IntraDayReporting.Domain.Entities;

namespace Petroineos.IntraDayReporting.Repository
{
    /// <summary>
    /// Represents file based aggregated Trading CSV report generated at a specified Date
    /// </summary>
    public class FileSystemAggregatedReportRepository : AggregatedReportBase
    {
        private readonly string _absolutePathCsvFile;

        public FileSystemAggregatedReportRepository(DateTime date, int period, string csvFile)
        {
            this.ReportDate = date;
            this.ReportPeriod = period;
            this._absolutePathCsvFile = csvFile;
        }

        public override Task<string> GetContents()
        {
            return Task.Run<string>(() => System.IO.File.ReadAllText(_absolutePathCsvFile));
        }

        public override string ToString()
        {
            return $"Date={ReportDate}  File={_absolutePathCsvFile}";
        }
    }
}
