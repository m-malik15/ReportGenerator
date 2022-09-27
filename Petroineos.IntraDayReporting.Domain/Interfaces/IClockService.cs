using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petroineos.IntraDayReporting.Domain.Interfaces
{
    public interface IClockService
    {
        DateTime GetCurrentTime();
    }
}
