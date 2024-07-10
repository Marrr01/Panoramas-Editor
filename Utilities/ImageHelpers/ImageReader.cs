using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageReader : IImageReader
    {
        public BitmapSource ReadAsBitmapSource(SelectedImage image)
        {
            using (var stream = File.OpenRead(image.FullPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}
