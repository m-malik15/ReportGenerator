using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Infrastructure
{
    public class JobConfig
    {
        public int MaximumSecondsDelayBetweenConsecutiveReportExtractions { get; set; }

        public string ReportsFolderPath { get; set; }
    }
}
