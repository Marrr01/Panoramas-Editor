using System;

namespace Panoramas_Editor
{
    internal class MathHelper : IMathHelper
    {
        /// <summary>
        /// Пропорционально переносит значение (value) из текущего диапазона значений (fromLow .. fromHigh) в новый диапазон
        /// </summary>
        /// <returns></returns>
        public double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh, int decimals = 2)
        {
            return Math.Round((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow, decimals);
        }
    }
}
