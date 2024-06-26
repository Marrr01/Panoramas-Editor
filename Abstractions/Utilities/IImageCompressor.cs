namespace Panoramas_Editor
{
    internal interface IImageCompressor
    {
        public SelectedImage CompressImage(SelectedDirectory newImageDirectory, SelectedImage originalImage);
        public SelectedImage CompressImageToThumbnail(SelectedDirectory newImageDirectory, SelectedImage originalImage);
    }
}
