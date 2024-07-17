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
        public ImageEditor(IImageReader imageReader, MathHelper mathHelper)
        {
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
            // новый центр изображения
            double centerX = _mathHelper.Map(horizontalOffset, -1.0, 1.0, 0, source.Width);
            double centerY = _mathHelper.Map(verticalOffset, -1.0, 1.0, 0, source.Height);

            // начало отрисовки
            double offsetX = centerX - source.Width / 2.0;
            double offsetY = centerY - source.Height / 2.0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, source.Width, source.Height));

                // смещенное изображение
                drawingContext.DrawImage(source, new Rect(-offsetX, -offsetY, source.Width, source.Height));

                // слой с обрезанной частью
                if (horizontalOffset < 0)
                {
                    double croppedWidth = Math.Abs(horizontalOffset) * source.Width;
                    int cropX = (int)(source.Width - croppedWidth);
                    if (cropX < 0) cropX = 0;
                    if (cropX + (int)croppedWidth > source.PixelWidth) croppedWidth = source.PixelWidth - cropX;

                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, 0, (int)croppedWidth, source.PixelHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY, croppedWidth, source.Height));
                }
                else if (horizontalOffset > 0)
                {
                    double croppedWidth = Math.Abs(horizontalOffset) * source.Width;
                    if (croppedWidth > source.Width) croppedWidth = source.Width;

                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, (int)croppedWidth, source.PixelHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY, croppedWidth, source.Height));
                }

                if (verticalOffset < 0)
                {
                    double croppedHeight = Math.Abs(verticalOffset) * source.Height;
                    int cropY = (int)(source.Height - croppedHeight);
                    if (cropY < 0) cropY = 0;
                    if (cropY + (int)croppedHeight > source.PixelHeight) croppedHeight = source.PixelHeight - cropY;

                    CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, cropY, source.PixelWidth, (int)croppedHeight));
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY - croppedHeight, source.Width, croppedHeight));
                }
                else if (verticalOffset > 0)
                {
                    double croppedHeight = Math.Abs(verticalOffset) * source.Height;
                    if (croppedHeight > source.Height) croppedHeight = source.Height;

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
                        int cropX = (int)(source.Width - croppedWidth);
                        int cropY = (int)(source.Height - croppedHeight);
                        if (cropX < 0) cropX = 0;
                        if (cropY < 0) cropY = 0;
                        if (cropX + (int)croppedWidth > source.PixelWidth) croppedWidth = source.PixelWidth - cropX;
                        if (cropY + (int)croppedHeight > source.PixelHeight) croppedHeight = source.PixelHeight - cropY;

                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, cropY, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY - croppedHeight, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset > 0 && verticalOffset > 0)
                    {
                        if (croppedWidth > source.Width) croppedWidth = source.Width;
                        if (croppedHeight > source.Height) croppedHeight = source.Height;

                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY + source.Height, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset < 0 && verticalOffset > 0)
                    {
                        int cropX = (int)(source.Width - croppedWidth);
                        if (cropX < 0) cropX = 0;
                        if (croppedWidth > source.Width) croppedWidth = source.Width;
                        if (croppedHeight > source.Height) croppedHeight = source.Height;

                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(cropX, 0, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - croppedWidth, -offsetY + source.Height, croppedWidth, croppedHeight));
                    }
                    else if (horizontalOffset > 0 && verticalOffset < 0)
                    {
                        int cropY = (int)(source.Height - croppedHeight);
                        if (cropY < 0) cropY = 0;
                        if (croppedWidth > source.Width) croppedWidth = source.Width;
                        if (croppedHeight > source.Height) croppedHeight = source.Height;

                        CroppedBitmap croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, cropY, (int)croppedWidth, (int)croppedHeight));
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + source.Width, -offsetY - croppedHeight, croppedWidth, croppedHeight));
                    }
                }
            }
            var transformedBitmap = new RenderTargetBitmap(
                (int)(source.PixelWidth / source.DpiX * 96),
                (int)(source.PixelHeight / source.DpiY * 96),
                96, 96, PixelFormats.Pbgra32);

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
