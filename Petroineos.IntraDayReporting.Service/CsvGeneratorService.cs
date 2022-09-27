using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petroineos.IntraDayReporting.Domain.Entities;
using Petroineos.IntraDayReporting.Domain.Interfaces;

namespace Petroineos.IntraDayReporting.Service
{
    public class CsvGeneratorService : ICsvGeneratorService
    {
        public string GenerateCsv(List<AggregatedTradePosition> trades)
        {
            var rows = trades.Select(t => new { LocalTime = $"{t.Hour:D2}:{t.Minute:D2}", t.Volume });
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
                csv.Flush();
                return System.Text.Encoding.UTF8.GetString(memory.ToArray());
            }
        }
    }
}
