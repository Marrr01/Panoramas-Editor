using System.Threading;

namespace Panoramas_Editor
{
    internal interface IImageEditor
    {
        /// <summary>
        /// Обрабатывает сжатое изображение из ImageSettings.Compressed
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public LoadedPreview EditCompressedImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct);

        /// <summary>
        /// Обрабатывает исходное изображение из ImageSettings.FullPath
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public SelectedImage EditOriginalImage(SelectedDirectory newImageDirectory, ImageSettings settings, CancellationToken ct, string newImageExtension);
    }
}
