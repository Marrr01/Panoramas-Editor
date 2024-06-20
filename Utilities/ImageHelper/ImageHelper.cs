using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageHelper : IImageCompressor, IBitmapConverter, IImageEditor, IImageReader
    {
        #region IImageCompressor
        public SelectedImage CompressImage(SelectedDirectory newImageDirectory, SelectedImage originalImage)
        {
            Thread.Sleep(3000);
            try
            {
                var compressedImagePath = Path.Combine(newImageDirectory.FullPath, $"compressed{{{Guid.NewGuid()}}}{originalImage.Extension}");
                File.Copy(originalImage.FullPath, compressedImagePath, true);
                return new SelectedImage(compressedImagePath);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message);
                return null;
            }
        }

        public SelectedImage CompressImageToThumbnail(SelectedDirectory newImageDirectory, SelectedImage originalImage)
        {
            Thread.Sleep(3000);
            try
            {
                var compressedImagePath = Path.Combine(newImageDirectory.FullPath, $"thumbnail{{{Guid.NewGuid()}}}{originalImage.Extension}");
                File.Copy(originalImage.FullPath, compressedImagePath, true);
                return new SelectedImage(compressedImagePath);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message);
                return null;
            }
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
        public SelectedImage EditCompressedImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct)
        {
            if (settings.Compressed != null &&
                settings.HorizontalOffset == 0 &&
                settings.VerticalOffset == 0)
            {
                return settings.Compressed;
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

            try
            {
                var editedImagePath = Path.Combine(newImageDirectory.FullPath, $"edited сompressed{{{Guid.NewGuid()}}}{settings.Extension}");
                File.Copy(settings.Compressed.FullPath, editedImagePath, true);
                return new SelectedImage(editedImagePath);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message);
                return null;
            }
        }

        public SelectedImage EditOriginalImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct, string newImageExtension)
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

            try
            {
                var editedImagePath = Path.Combine(newImageDirectory.FullPath, $"edited original{{{Guid.NewGuid()}}}{settings.Extension}");
                File.Copy(settings.FullPath, editedImagePath, true);
                return new SelectedImage(editedImagePath);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message);
                return null;
            }
        }
        #endregion

        #region IImageReader
        public BitmapImage ReadAsBitmapImage(SelectedImage image)
        {
            using (var stream = File.OpenRead(image.FullPath))
            {
                //Thread.Sleep(1000); // удалить потом
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public Bitmap ReadAsBitmap(SelectedImage image)
        {
            Thread.Sleep(1000);
            return null;
        }
        #endregion
    }
}

