using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Entities
{
    public class TradePosition
    {
        public TradePosition(DateTime date, IEnumerable<TradePeriod> periods)
        {
            this.Date = date;
            this.Periods = periods.ToList();
        }

     
        public DateTime Date { get; }

       
        public List<TradePeriod> Periods { get; }

        public override string ToString()
        {
            return $"Date={Date}    Periods={this.Periods?.Count}";
        }
    }
}
