using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageSettings : SelectedFile, IEquatable<ImageSettings>
    {
        #region static
        // Список стандартных расширений, которые должны обрабатываться на любой машине:
        // https://learn.microsoft.com/ru-ru/windows/win32/wic/-wic-about-windows-imaging-codec?redirectedfrom=MSDN
        public static List<string> ValidExtensions = new List<string>
        {
            ".bmp",
            ".gif",
            //".ico", /*Нет предустановленного кодировщика*/
            ".jpeg",
            ".jpe",
            ".jpg",
            ".jxr",
            ".png",
            ".tiff",
            ".tif",
            ".wdp",
            ".dds"
        };
        public static string ValidExtensionsAsString()
        {
            var sb = new StringBuilder();
            foreach (var extension in ValidExtensions)
            {
                sb.Append($"{extension};");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
        #endregion

        private double _horizontalOffset;
        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                if (_horizontalOffset != value)
                {
                    _horizontalOffset = value;
                    ResultPreview = null;
                    OnPropertyChanged();
                }
            }
        }

        private double _verticalOffset;
        public double VerticalOffset

        {
            get => _verticalOffset;
            set 
            { 
                if (_verticalOffset != value)
                {
                    _verticalOffset = value;
                    ResultPreview = null;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage ResultPreview { get; set; }
        //private BitmapImage _resultPreview;
        //public BitmapImage ResultPreview
        //{
        //    get => _resultPreview;
        //    set
        //    {
        //        ResultPreview = value;
        //        ResultPreviewChanged?.Invoke(this, EventArgs.Empty);
        //        OnPropertyChanged();
        //    }
        //}
        //public event EventHandler? ResultPreviewChanged;

        private BitmapImage _compressedBitmapImage;
        public BitmapImage CompressedBitmapImage
        {
            get => _compressedBitmapImage;
            set
            {
                _compressedBitmapImage = value;
                CompressedBitmapImageChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
        public event EventHandler? CompressedBitmapImageChanged;

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
            if (!ValidExtensions.Contains(Extension))
            {
                throw new ArgumentException($"Файлы с расширением {Extension} не поддерживаются:\n{fullPath}");
            }
            HorizontalOffset = 0;
            VerticalOffset = 0;
            IsMarked = false;
        }

        public bool Equals(ImageSettings other)
        {
            return this.FullPath == other.FullPath ? true : false;
        }
    }
}
