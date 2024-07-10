using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageSettings : SelectedImage, IDisposable
    {
        private double _horizontalOffset;
        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                if (_horizontalOffset != value)
                {
                    _horizontalOffset = value;
                    HorizontalOffsetChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }
        public event EventHandler? HorizontalOffsetChanged;

        private double _verticalOffset;
        public double VerticalOffset
        {
            get => _verticalOffset;
            set 
            { 
                if (_verticalOffset != value)
                {
                    _verticalOffset = value;
                    VerticalOffsetChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                }
            }
        }
        public event EventHandler? VerticalOffsetChanged;

        private BitmapSource _thumbnailBitmapImage;
        public BitmapSource ThumbnailBitmapImage
        {
            get => _thumbnailBitmapImage;
            set
            {
                _thumbnailBitmapImage = value;
                OnPropertyChanged();
            }
        }

        public SelectedImage Thumbnail { get; set; }

        private SelectedImage _compressed;
        public SelectedImage Compressed
        {
            get => _compressed;
            set
            {
                _compressed = value;
                CompressedChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
        public event EventHandler? CompressedChanged;

        private bool isMarked;
        public bool IsMarked
        {
            get => isMarked;
            set
            {
                isMarked = value;
                OnPropertyChanged();
            }
        }

        public ImageSettings(string fullPath) : base(fullPath)
        {
            HorizontalOffset = 0;
            VerticalOffset = 0;
            IsMarked = false;
        }

        public void Dispose()
        {
            try { File.Delete(Thumbnail.FullPath); }
            catch { }
            try { File.Delete(Compressed.FullPath); }
            catch { }
        }
    }
}
