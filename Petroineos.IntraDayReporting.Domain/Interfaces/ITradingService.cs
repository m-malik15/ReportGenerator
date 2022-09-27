using Petroineos.IntraDayReporting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Interfaces
{
    public interface ITradingService
    {
        Task<List<TradePosition>> GetTradesAsync(System.DateTime date);
    }
}
