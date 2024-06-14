using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageHelper : IImageCompressor, IBitmapConverter, IImageEditor
    {
        #region IImageCompressor
        public BitmapImage GetCompressedBitmapImage(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                Thread.Sleep(3000);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public Bitmap GetCompressedBitmap(string path)
        {
            Thread.Sleep(3000);
            return null;
        }
        #endregion

        #region IBitmapConverter
        public BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            Thread.Sleep(3000);
            return null;

            ////Скопировал с Stack Overflow, надо тестить:
            //using (var memory = new MemoryStream())
            //{
            //    bitmap.Save(memory, ImageFormat.Png);
            //    memory.Position = 0;

            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = memory;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();
            //    bitmapImage.Freeze();

            //    return bitmapImage;
            //}
        }

        public Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            Thread.Sleep(3000);
            return null;

            ////Скопировал с Stack Overflow, надо тестить:
            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            //    enc.Save(outStream);
            //    Bitmap bitmap = new Bitmap(outStream);

            //    return new Bitmap(bitmap);
            //}
        }

        public byte[] BitmapImage2ByteArray(BitmapImage bitmapImage)
        {
            Thread.Sleep(3000);
            return null;

            ////Скопировал с Stack Overflow, надо тестить:
            //byte[] data;
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    encoder.Save(ms);
            //    data = ms.ToArray();
            //}
            //return data;
        }
        #endregion

        #region IImageEditor
        public BitmapImage EditCompressedBitmapImage(ImageSettings settings, CancellationToken ct)
        {
            if (settings.CompressedBitmapImage != null &&
                settings.HorizontalOffset == 0 &&
                settings.VerticalOffset == 0)
            {
                return settings.CompressedBitmapImage;
            }

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            return settings.CompressedBitmapImage;
        }

        public Bitmap EditOriginalImage(ImageSettings settings, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);

            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }
            return null;
        }

        public void Save(Bitmap edited, SelectedFile newFilesDirectory, string newFileName, string newExtension)
        {
            Thread.Sleep(3000);
        }
        #endregion
    }
}

