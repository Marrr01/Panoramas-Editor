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
        private MathHelper _mathHelper;

        private readonly double MIN_OFFSET;
        private readonly double MAX_OFFSET;
        private readonly double CENTER;
        public ImageEditor(IImageReader imageReader, MathHelper mathHelper)
        {
            MIN_OFFSET = double.Parse(App.Current.Configuration["min"]);
            MAX_OFFSET = double.Parse(App.Current.Configuration["max"]);
            CENTER = double.Parse(App.Current.Configuration["center"]);

            _imageReader = imageReader;
            _mathHelper = mathHelper;
        }

        public BitmapSource GetPreview(ImageSettings settings,
                                       CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var transformedBitmap = ApplyOffsets(_imageReader.ReadAsBitmapSource(settings.Compressed),
                                                 settings.HorizontalOffset,
                                                 settings.VerticalOffset);
            transformedBitmap.Freeze();
            return transformedBitmap;
        }

        public SelectedImage EditOriginalImage(SelectedDirectory newImageDirectory,
                                               ImageSettings settings,
                                               CancellationToken ct,
                                               string newImageExtension)
        {
            ct.ThrowIfCancellationRequested();
            var editedImagePath = Path.Combine(newImageDirectory.FullPath,
                                               $"{settings.FileNameWithoutExtension}[{settings.HorizontalOffset};{settings.VerticalOffset}]{newImageExtension}");
            var transformedBitmap = ApplyOffsets(_imageReader.ReadAsBitmapSource(settings),
                                                 settings.HorizontalOffset,
                                                 settings.VerticalOffset);
            ct.ThrowIfCancellationRequested();
            using (var destStream = new FileStream(editedImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BitmapEncoder encoder = GetEncoder(newImageExtension);
                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
                    encoder.Save(destStream);
                }
            }
            return new SelectedImage(editedImagePath);
        }

        private BitmapSource ApplyOffsets(BitmapSource source, double horizontalOffset, double verticalOffset)
        {
            double centerX = _mathHelper.Map(horizontalOffset, MIN_OFFSET, MAX_OFFSET, 0, source.Width, 0);
            double centerY = _mathHelper.Map(verticalOffset, MIN_OFFSET, MAX_OFFSET, 0, source.Height, 0);

            double offsetX = centerX - source.Width / 2.0;
            double offsetY = centerY - source.Height / 2.0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, source.Width, source.Height));

                drawingContext.DrawImage(source, new Rect(-offsetX, -offsetY, source.Width, source.Height));

                if (horizontalOffset < CENTER)
                {
                    double croppedWidth = source.Width * Math.Abs(horizontalOffset);
                    if (croppedWidth > 0)
                    {
                        int cropX = (int)(source.Width - croppedWidth);
                        cropX = Math.Max(0, cropX);
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width - cropX);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, 0, cropWidth, source.PixelHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY, croppedWidth, source.Height));
                    }
                }
                else if (horizontalOffset > CENTER)
                {
                    double croppedWidth = source.Width * Math.Abs(horizontalOffset);
                    if (croppedWidth > 0)
                    {
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, cropWidth, source.PixelHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY, cropWidth, source.Height));
                    }
                }

                if (verticalOffset < CENTER)
                {
                    double croppedHeight = source.Height * Math.Abs(verticalOffset);
                    if (croppedHeight > 0)
                    {
                        int cropY = (int)(source.Height - croppedHeight);
                        cropY = Math.Max(0, cropY);
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height - cropY);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, cropY, source.PixelWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY - croppedHeight, source.Width, croppedHeight));
                    }
                }
                else if (verticalOffset > CENTER)
                {
                    double croppedHeight = source.Height * Math.Abs(verticalOffset);
                    if (croppedHeight > 0)
                    {
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, source.PixelWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY + source.Height, source.Width, cropHeight));
                    }
                }

                if (horizontalOffset != CENTER && verticalOffset != CENTER)
                {
                    double croppedWidth = source.Width * Math.Abs(horizontalOffset);
                    double croppedHeight = source.Height * Math.Abs(verticalOffset);
                    if (horizontalOffset < CENTER && verticalOffset < CENTER)
                    {
                        int cropX = (int)(source.Width - croppedWidth);
                        int cropY = (int)(source.Height - croppedHeight);
                        cropX = Math.Max(0, cropX);
                        cropY = Math.Max(0, cropY);
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width - cropX);
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height - cropY);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, cropY, cropWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY - croppedHeight, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset > CENTER && verticalOffset > CENTER)
                    {
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width);
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, cropWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY + source.Height, cropWidth, cropHeight));
                    }
                    else if (horizontalOffset < CENTER && verticalOffset > CENTER)
                    {
                        int cropX = (int)(source.Width - croppedWidth);
                        cropX = Math.Max(0, cropX);
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width - cropX);
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, 0, cropWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY + source.Height, cropWidth, cropHeight));
                    }
                    else if (horizontalOffset > CENTER && verticalOffset < CENTER)
                    {
                        int cropY = (int)(source.Height - croppedHeight);
                        cropY = Math.Max(0, cropY);
                        int cropWidth = (int)Math.Min(croppedWidth, source.Width);
                        int cropHeight = (int)Math.Min(croppedHeight, source.Height - cropY);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, cropY, cropWidth, cropHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY - croppedHeight, cropWidth, cropHeight));
                    }
                }
            }

            var transformedBitmap = new RenderTargetBitmap(
                source.PixelWidth,
                source.PixelHeight,
                source.DpiX, source.DpiY, PixelFormats.Pbgra32);

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