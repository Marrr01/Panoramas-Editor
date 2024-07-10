using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IImageReader
    {
        public BitmapSource ReadAsBitmapSource(SelectedImage image);
    }
}
