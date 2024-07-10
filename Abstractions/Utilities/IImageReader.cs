using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IImageReader
    {
        public BitmapImage ReadAsBitmapImage(SelectedImage image);
    }
}
