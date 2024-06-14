using System.Drawing;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IImageCompressor
    {
        public BitmapImage GetCompressedBitmapImage(string path);
        public Bitmap GetCompressedBitmap(string path);
    }
}
