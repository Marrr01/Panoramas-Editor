using System;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Panoramas_Editor
{
    internal class ImageEditor : IImageEditor
    {
        public SelectedImage EditCompressedImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct)
        {
            return EditImage(newImageDirectory, settings, ct, settings.Compressed);
        }

        public SelectedImage EditOriginalImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct, string newImageExtension)
        {
            var originalImage = new SelectedImage(Path.ChangeExtension(settings.FullPath, newImageExtension));
            return EditImage(newImageDirectory, settings, ct, originalImage);
        }

        private SelectedImage EditImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct, SelectedImage sourceImage)
        {
            if (sourceImage == null || settings.HorizontalOffset == 0 && settings.VerticalOffset == 0)
            {
                return sourceImage;
            }

            ct.ThrowIfCancellationRequested();

            try
            {
                var editedImagePath = Path.Combine(newImageDirectory.FullPath, $"editedImage_{Guid.NewGuid()}{settings.Extension}");
                using (var sourceStream = new FileStream(sourceImage.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = sourceStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    var transformedBitmap = ApplyOffsets(bitmap, settings.HorizontalOffset, settings.VerticalOffset);

                    using (var destStream = new FileStream(editedImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
                        encoder.Save(destStream);
                    }
                }
                return new SelectedImage(editedImagePath);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }
        }
        private BitmapSource ApplyOffsets(BitmapSource source, double horizontalOffset, double verticalOffset)
        {
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(horizontalOffset, verticalOffset));
            var transformedBitmap = new TransformedBitmap(source, transformGroup);
            return transformedBitmap; 
        }
    }
}
