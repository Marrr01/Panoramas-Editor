using System;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    /// <summary>
    /// Заглушка для тестов
    /// </summary>
    internal class ImageHelperStub : IImageCompressor, IImageEditor, IImageReader
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

        #region IImageEditor
        public SelectedImage EditCompressedImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct)
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
                Thread.Sleep(1000);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
        #endregion
    }
}

