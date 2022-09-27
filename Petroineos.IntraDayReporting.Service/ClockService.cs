using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petroineos.IntraDayReporting.Domain.Interfaces;

namespace Petroineos.IntraDayReporting.Service
{
    public class ClockService : IClockService
    {
        private const string BritishTimeZone = "Greenwich Standard Time";


        public DateTime GetCurrentTime()
        {
            var zoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(BritishTimeZone);
            var gmtClockTime = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zoneInfo);
            return gmtClockTime;
        }
    }
}
