using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Panoramas_Editor
{
    internal class ImageCompressor : IImageCompressor
    {
        //сжать + сохранить в новой директории
        public SelectedImage CompressImage(SelectedDirectory newImageDirectory, SelectedImage originalImage)
        {
            var newFilePath = Path.Combine(newImageDirectory.FullPath, $"compressed{{{Guid.NewGuid()}}}.jpeg");
            using (var image = Image.FromFile(originalImage.FullPath))
            {
                //Zero would give you the lowest quality image and 100 the highest
                SaveJpeg(newFilePath, image, 50);
            }
            return new SelectedImage(newFilePath);
        }

        //создать миниатюру + сохранить в новой директории
        public SelectedImage CompressImageToThumbnail(SelectedDirectory newImageDirectory, SelectedImage originalImage)
        {
            string newFilePath = Path.Combine(newImageDirectory.FullPath, $"thumbnail{{{Guid.NewGuid()}}}.jpeg");
            using (var image = Image.FromFile(originalImage.FullPath))
            {
                int thumbnailSize = 100;
                int height, width;
                if(image.Width > image.Height)
                {
                    width = thumbnailSize;
                    height = (int)(image.Height * thumbnailSize / (double)image.Width);
                }
                else
                {
                    height = thumbnailSize;
                    width = (int)(image.Width * thumbnailSize / (double)image.Height);
                }
                using (var thumbnail = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero))
                {
                    thumbnail.Save(newFilePath, ImageFormat.Jpeg);
                }
            }
            return new SelectedImage(newFilePath);
        }

        private void SaveJpeg(string path, Image img, long quality)
        {
            using (var encoderParameters = new EncoderParameters(1))
            {
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                ImageCodecInfo codec = GetEncoderInfo("image/jpeg");
                if (codec != null) { img.Save(path, codec, encoderParameters); }
                else { throw new InvalidOperationException("Ошибка при сжатии изображения");}
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs?.FirstOrDefault(codec => codec.MimeType == mimeType);
        }
    }
}
