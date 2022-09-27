using Petroineos.IntraDayReporting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Interfaces
{
    public interface IReportsRepo
    {
        /// <summary>
        /// Returns a list of all report names
        /// </summary>
        /// <returns></returns>
        Task<List<AggregatedReportBase>> GetReports();

        /// <summary>
        /// Saves a report into the repository
        /// </summary>
        /// <param name="reportDateTime">The report contains trade aggregations upto this specified date time</param>
        /// <param name="contents">The raw contents of the file in CSV format</param>
        /// <returns></returns>
        Task SaveReport(DateTime reportDateTime, string contents);
    }
}
