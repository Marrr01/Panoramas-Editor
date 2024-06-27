using System;

namespace Panoramas_Editor
{
    internal class MathHelper
    {
        /// <summary>
        /// Пропорционально переносит значение (value) из текущего диапазона значений (fromLow .. fromHigh) в новый диапазон
        /// </summary>
        /// <returns></returns>
        public double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh, int decimals = 3)
        {
            return Math.Round((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow, decimals);
        }
    }
}
