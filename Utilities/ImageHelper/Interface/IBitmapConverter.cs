using System.Drawing;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IBitmapConverter
    {
        public Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage);
        public BitmapImage Bitmap2BitmapImage(Bitmap bitmap);
        public byte[] BitmapImage2ByteArray(BitmapImage bitmapImage);
        //public byte[] Bitmap2ByteArray(Bitmap bitmap);
    }
}
