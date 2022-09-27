using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petroineos.IntraDayReporting.Domain.Entities;

namespace Petroineos.IntraDayReporting.Domain.Interfaces
{
    public interface ICsvGeneratorService
    {
        string GenerateCsv(List<AggregatedTradePosition> trades);
    }
}
