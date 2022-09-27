using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Entities
{
    public class TradePeriod
    {
        public TradePeriod(int period, double volume)
        {
            Period = period;
            Volume = volume;
        }
       
        public int Period { get; }
       
        public double Volume { get; }

        public override string ToString()
        {
            return $"Period={this.Period}   Volume={this.Volume}";
        }
    }
}
