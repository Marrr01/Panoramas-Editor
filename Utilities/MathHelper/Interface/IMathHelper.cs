namespace Panoramas_Editor
{
    internal interface IMathHelper
    {
        public double Map(double value, double fromLow, double fromHigh, double toLow, double toHigh, int decimals = 2);
    }
}
