namespace Panoramas_Editor
{
    internal class LoadedPreview : SelectedImage
    {
        public double HorizontalOffset;
        public double VerticalOffset;
        public LoadedPreview(string fullPath, double horizontalOffset, double verticalOffset) : base(fullPath)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }
    }
}
