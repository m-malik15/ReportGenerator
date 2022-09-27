using Petroineos.IntraDayReporting.Domain.Entities;
using Petroineos.IntraDayReporting.Domain.Infrastructure;
using Petroineos.IntraDayReporting.Domain.Interfaces;

namespace Petroineos.IntraDayReporting.Service
{
    public class TradingService : ITradingService
    {
        private readonly JobConfig _config;
        private readonly Services.PowerService _powerService = null;

        public TradingService(JobConfig config)
        {
            _powerService = new Services.PowerService();
            _config = config;
        }

        public async Task<List<TradePosition>> GetTradesAsync(DateTime date)
        {
            var trades = (await _powerService.GetTradesAsync(date)).ToList();
            var results = new List<TradePosition>();
            trades.ForEach(trade =>
            {
                var periods = trade
                .Periods
                .Select(tp => new TradePeriod(tp.Period, tp.Volume))
                .ToList();
                var tradePosition = new TradePosition(date, periods);
                results.Add(tradePosition);
            });
            return results;

        }

    }
}


