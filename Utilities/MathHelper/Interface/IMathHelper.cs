using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panoramas_Editor
{
    internal interface IMathHelper
    {
        public double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh, int decimals = 2);
    }
}
