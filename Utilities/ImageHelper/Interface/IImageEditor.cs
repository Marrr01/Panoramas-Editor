using System.Drawing;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal interface IImageEditor
    {
        /// <summary>
        /// Обрабатывает сжатое изображение из ImageSettings.CompressedBitmapImage
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public BitmapImage EditCompressedBitmapImage(ImageSettings settings, CancellationToken ct);

        /// <summary>
        /// Обрабатывает исходное изображение из ImageSettings.FullPath
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Bitmap EditOriginalImage(ImageSettings settings, CancellationToken ct);

        /// <summary>
        /// Записывает обработанное изображение на диск
        /// </summary>
        /// <param name="edited"></param>
        /// <param name="newFilesDirectory"></param>
        /// <param name="newFileName"></param>
        /// <param name="newExtension"></param>
        public void Save(Bitmap edited, SelectedFile newFilesDirectory, string newFileName, string newExtension);
    }
}
