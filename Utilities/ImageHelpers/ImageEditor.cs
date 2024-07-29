using NLog;
using NPOI.SS.Formula.Functions;
using System;
using System.IO;
using System.Linq;
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
        private readonly string TEMP_FILES_DIR;
        private Logger _logger => App.Current.Logger;
        private bool _debug;

        public ImageEditor(IImageReader imageReader, MathHelper mathHelper)
        {
            MIN_OFFSET = double.Parse(App.Current.Configuration["min"]);
            MAX_OFFSET = double.Parse(App.Current.Configuration["max"]);
            CENTER = double.Parse(App.Current.Configuration["center"]);
            TEMP_FILES_DIR = App.Current.Configuration["temp"];
            _imageReader = imageReader;
            _mathHelper = mathHelper;
            _debug = false;
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
            if (_debug)
            {
                var debugImages = from file in Directory.GetFiles(TEMP_FILES_DIR)
                                  where Path.GetFileName(file).StartsWith("[DEBUG]")
                                  select file;

                foreach (var debugImage in debugImages)
                {
                    File.Delete(debugImage);
                    _logger.Debug($"{debugImage} удален");
                }
            }

            var sourceHeight = source.PixelHeight;
            var sourceWidth  = source.PixelWidth;

            //Координаты нового центра
            var newCenterX = _mathHelper.Map(horizontalOffset, MIN_OFFSET, MAX_OFFSET, 0, sourceWidth, 0);
            var newCenterY = _mathHelper.Map(verticalOffset, MIN_OFFSET, MAX_OFFSET, 0, sourceHeight, 0);

            //Смещение относительно старого центра
            var offsetX = (int)Math.Round(newCenterX - sourceWidth / 2.0, 0);
            var offsetY = (int)Math.Round(newCenterY - sourceHeight / 2.0, 0);

            //Смещение относительно старого центра по модулю
            var absOffsetX = Math.Abs(offsetX);
            var absOffsetY = Math.Abs(offsetY);

            var transformedBitmap = new RenderTargetBitmap(
                sourceWidth,
                sourceHeight,
                96, 96, PixelFormats.Pbgra32);

            #region step 0
            var initDrawing = new DrawingVisual();
            using (var drawingContext = initDrawing.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, sourceWidth, sourceHeight));
                drawingContext.DrawImage(source, new Rect(-offsetX, -offsetY, sourceWidth, sourceHeight));
            }
            transformedBitmap.Render(initDrawing);
            SaveBitmapSourceDebug(transformedBitmap, "step 0 result");
            #endregion

            #region step 1
            if (offsetX < 0)
            {
                var drawing = new DrawingVisual();
                using (var drawingContext = drawing.RenderOpen())
                {
                    int x = (sourceWidth - absOffsetX);
                    var croppedBitmap = new CroppedBitmap(source, new Int32Rect(x, 0, absOffsetX, sourceHeight));
                    SaveBitmapSourceDebug(croppedBitmap, "step 1 cropped");
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - absOffsetX, -offsetY, absOffsetX, sourceHeight));
                }
                transformedBitmap.Render(drawing);
                SaveBitmapSourceDebug(transformedBitmap, "step 1 result");
            }
            else if (offsetX > 0)
            {
                var drawing = new DrawingVisual();
                using (var drawingContext = drawing.RenderOpen())
                {
                    var croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, absOffsetX, sourceHeight));
                    SaveBitmapSourceDebug(croppedBitmap, "step 1 cropped");
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + sourceWidth, -offsetY, absOffsetX, sourceHeight));
                }
                transformedBitmap.Render(drawing);
                SaveBitmapSourceDebug(transformedBitmap, "step 1 result");
            }
            #endregion

            #region step 2
            if (offsetY < 0)
            {
                var drawing = new DrawingVisual();
                using (var drawingContext = drawing.RenderOpen())
                {
                    int y = (sourceHeight - absOffsetY);
                    var croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, y, sourceWidth, absOffsetY));
                    SaveBitmapSourceDebug(croppedBitmap, "step 2 cropped");
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY - absOffsetY, sourceWidth, absOffsetY));
                }
                transformedBitmap.Render(drawing);
                SaveBitmapSourceDebug(transformedBitmap, "step 2 result");
            }
            else if (offsetY > 0)
            {
                var drawing = new DrawingVisual();
                using (var drawingContext = drawing.RenderOpen())
                {
                    var croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, sourceWidth, absOffsetY));
                    SaveBitmapSourceDebug(croppedBitmap, "step 2 cropped");
                    drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX, -offsetY + sourceHeight, sourceWidth, absOffsetY));
                }
                transformedBitmap.Render(drawing);
                SaveBitmapSourceDebug(transformedBitmap, "step 2 result");
            }
            #endregion

            #region step 3
            if (absOffsetX != 0 && absOffsetY != 0)
            {
                if (offsetX < 0 && offsetY < 0)
                {
                    var drawing = new DrawingVisual();
                    using (var drawingContext = drawing.RenderOpen())
                    {
                        int x = (sourceWidth - absOffsetX);
                        int y = (sourceHeight - absOffsetY);
                        var croppedBitmap = new CroppedBitmap(source, new Int32Rect(x, y, absOffsetX, absOffsetY));
                        SaveBitmapSourceDebug(croppedBitmap, "step 3 cropped");
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - absOffsetX, -offsetY - absOffsetY, absOffsetX, absOffsetY));
                    }
                    transformedBitmap.Render(drawing);
                    SaveBitmapSourceDebug(transformedBitmap, "step 3 result");
                }
                else if (offsetX > 0 && offsetY > 0)
                {
                    var drawing = new DrawingVisual();
                    using (var drawingContext = drawing.RenderOpen())
                    {
                        var croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, absOffsetX, absOffsetY));
                        SaveBitmapSourceDebug(croppedBitmap, "step 3 cropped");
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + sourceWidth, -offsetY + sourceHeight, absOffsetX, absOffsetY));
                    }
                    transformedBitmap.Render(drawing);
                    SaveBitmapSourceDebug(transformedBitmap, "step 3 result");
                }
                else if (offsetX < 0 && offsetY > 0)
                {
                    var drawing = new DrawingVisual();
                    using (var drawingContext = drawing.RenderOpen())
                    {
                        int x = (sourceWidth - absOffsetX);
                        var croppedBitmap = new CroppedBitmap(source, new Int32Rect(x, 0, absOffsetX, absOffsetY));
                        SaveBitmapSourceDebug(croppedBitmap, "step 3 cropped");
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX - absOffsetX, -offsetY + sourceHeight, absOffsetX, absOffsetY));
                    }
                    transformedBitmap.Render(drawing);
                    SaveBitmapSourceDebug(transformedBitmap, "step 3 result");
                }
                else if (offsetX > 0 && offsetY < 0)
                {
                    var drawing = new DrawingVisual();
                    using (var drawingContext = drawing.RenderOpen())
                    {
                        int y = (sourceHeight - absOffsetY);
                        var croppedBitmap = new CroppedBitmap(source, new Int32Rect(0, y, absOffsetX, absOffsetY));
                        SaveBitmapSourceDebug(croppedBitmap, "step 3 cropped");
                        drawingContext.DrawImage(croppedBitmap, new Rect(-offsetX + sourceWidth, -offsetY - absOffsetY, absOffsetX, absOffsetY));
                    }
                    transformedBitmap.Render(drawing);
                    SaveBitmapSourceDebug(transformedBitmap, "step 3 result");
                }
            }
            #endregion

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

        private void SaveBitmapSourceDebug(BitmapSource source, string fileName)
        {
            if (!_debug) return;

            var newImagePath = Path.Combine(TEMP_FILES_DIR, $"[DEBUG] {fileName}.jpg");
            using (var destStream = new FileStream(newImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BitmapEncoder encoder = GetEncoder(".jpg");
                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(source));
                    encoder.Save(destStream);
                    _logger.Debug($"{newImagePath} сохранен");
                }
            }
        }
    }
}