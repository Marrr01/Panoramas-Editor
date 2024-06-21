using System.Drawing;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IImageReader
    {
        public BitmapImage ReadAsBitmapImage(SelectedImage image);
        //public Bitmap ReadAsBitmap(SelectedImage image);
    }
}
