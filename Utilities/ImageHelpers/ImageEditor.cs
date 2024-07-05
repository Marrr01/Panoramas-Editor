using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageEditor : IImageEditor
    {
        private IImageReader _imageReader;
        public ImageEditor(IImageReader imageReader)
        {
            _imageReader = imageReader;
        }

        public LoadedPreview EditCompressedImage(SelectedDirectory newImageDirectory,
                                                 ImageSettings settings,
                                                 CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var horizontalOffset = settings.HorizontalOffset;
            var verticalOffset = settings.VerticalOffset;
            var extension = settings.Compressed.Extension;

            var result = EditImage(newImageDirectory, settings.Compressed, horizontalOffset, verticalOffset, extension, ct);

            return new LoadedPreview(result, horizontalOffset, verticalOffset);
        }

        public SelectedImage EditOriginalImage(SelectedDirectory newImageDirectory,
                                               ImageSettings settings,
                                               CancellationToken ct,
                                               string newImageExtension)
        {
            ct.ThrowIfCancellationRequested();

            var horizontalOffset = settings.HorizontalOffset;
            var verticalOffset = settings.VerticalOffset;
            var extension = settings.Extension;

            var result = EditImage(newImageDirectory, settings, horizontalOffset, verticalOffset, newImageExtension, ct);
            return new SelectedImage(result);
        }

        private string EditImage(SelectedDirectory newImageDirectory,
                                 SelectedImage sourceImage,
                                 double horizontalOffset,
                                 double verticalOffset,
                                 string extension,
                                 CancellationToken ct)
        {
            var editedImagePath = Path.Combine(newImageDirectory.FullPath, $"{sourceImage.FileNameWithoutExtension}[{horizontalOffset};{verticalOffset}]{extension}");
            var transformedBitmap = ApplyOffsets(_imageReader.ReadAsBitmapImage(sourceImage), horizontalOffset, verticalOffset);
            using (var destStream = new FileStream(editedImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BitmapEncoder encoder = GetEncoder(extension);
                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
                    encoder.Save(destStream);
                }
            }
            return editedImagePath;
        }

        private BitmapSource ApplyOffsets(BitmapSource source,
                                          double horizontalOffset,
                                          double verticalOffset)
        {
            // новый центр изображения
            double centerX = source.Width / 2.0 + horizontalOffset * source.Width / 2.0;
            double centerY = source.Height / 2.0 + verticalOffset * source.Height / 2.0;

            // начало отрисовки
            double offsetX = centerX - source.Width / 2.0;
            double offsetY = centerY - source.Height / 2.0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, source.Width, source.Height));

                //смещенное изображение
                drawingContext.DrawImage(source, new Rect(-offsetX, -offsetY, source.Width, source.Height));

                //слой с обрезанной частью
                if (horizontalOffset < 0)
                {
                    double croppedWidth = Math.Abs(horizontalOffset) * source.Width;
                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect((int)(source.Width - croppedWidth), 0, (int)croppedWidth, source.PixelHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY, croppedWidth, source.Height));
                }
                else if (horizontalOffset > 0)
                {
                    double croppedWidth = Math.Abs(horizontalOffset) * source.Width;
                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, (int)croppedWidth, source.PixelHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY, croppedWidth, source.Height));
                }

                if (verticalOffset < 0)
                {
                    double croppedHeight = Math.Abs(verticalOffset) * source.Height;
                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, (int)(source.Height - croppedHeight), source.PixelWidth, (int)croppedHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY - croppedHeight, source.Width, croppedHeight));
                }
                else if (verticalOffset > 0)
                {
                    double croppedHeight = Math.Abs(verticalOffset) * source.Height;
                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, source.PixelWidth, (int)croppedHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY + source.Height, source.Width, croppedHeight));
                }

                // обработка углов
                if (horizontalOffset != 0 && verticalOffset != 0)
                {
                    double croppedWidth = Math.Abs(horizontalOffset) * source.Width;
                    double croppedHeight = Math.Abs(verticalOffset) * source.Height;

                    if (horizontalOffset < 0 && verticalOffset < 0)
                    {
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect((int)(source.Width - croppedWidth), (int)(source.Height - croppedHeight), (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY - croppedHeight, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset > 0 && verticalOffset > 0)
                    {
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY + source.Height, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset < 0 && verticalOffset > 0)
                    {
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect((int)(source.Width - croppedWidth), 0, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY + source.Height, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset > 0 && verticalOffset < 0)
                    {
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, (int)(source.Height - croppedHeight), (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY - croppedHeight, croppedWidth, croppedHeight));
                    }
                }
            }

            RenderTargetBitmap transformedBitmap = new RenderTargetBitmap((int)source.Width, (int)source.Height, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            transformedBitmap.Render(drawingVisual);
            return transformedBitmap;
        }

        private BitmapEncoder GetEncoder(string extension)
        {
            switch (extension.ToLower())
            {
                case ".png":
                    return new PngBitmapEncoder();
                case ".jpeg":
                case ".jpg":
                case ".jpe":
                case ".jxr":
                    return new JpegBitmapEncoder();
                case ".bmp":
                    return new BmpBitmapEncoder();
                case ".gif":
                    return new GifBitmapEncoder();
                case ".tiff":
                case ".tif":
                    return new TiffBitmapEncoder();
                default:
                    throw new ArgumentException($"{extension} не поддерживается");
            }
        }
    }
}
