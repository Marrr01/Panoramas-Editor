using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class ImageCenterSelectorVM : ObservableObject, IDataErrorInfo
    {
        #region Actual - значения изображения в окне
        private double actualHeight_;
        public double ActualHeight_
        {
            get => actualHeight_;
            set
            {
                actualHeight_ = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedActualVerticalValue));
            }
        }

        private double actualWidth_;
        public double ActualWidth_
        {
            get => actualWidth_;
            set
            {
                actualWidth_ = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedActualHorizontalValue));
            }
        }

        public double SelectedActualVerticalValue
        {
            get => _mathHelper.Map(SelectedVerticalValue, MIN_OFFSET, MAX_OFFSET, 0, ActualHeight_, DECIMALS);
            set
            {
                SelectedVerticalValue = _mathHelper.Map(value, 0, ActualHeight_, MIN_OFFSET, MAX_OFFSET, DECIMALS);
                OnPropertyChanged(nameof(SelectedVerticalValue));
                OnPropertyChanged();
            }
        }

        public double SelectedActualHorizontalValue
        {
            get => _mathHelper.Map(SelectedHorizontalValue, MIN_OFFSET, MAX_OFFSET, 0, ActualWidth_, DECIMALS);
            set
            {
                SelectedHorizontalValue = _mathHelper.Map(value, 0, ActualWidth_, MIN_OFFSET, MAX_OFFSET, DECIMALS);
                OnPropertyChanged(nameof(SelectedHorizontalValue));
                OnPropertyChanged();
            }
        }
        #endregion

        #region НЕ Actual
        public ImageSettings ImageSettings { get; }

        private BitmapSource _bitmap;
        public BitmapSource? Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                OnPropertyChanged();
            }
        }
        public double SelectedVerticalValue
        {
            get => ImageSettings.VerticalOffset;
            set
            {
                ImageSettings.VerticalOffset = value;
                _selectedVerticalValueBox = value.ToString();
                OnPropertyChanged(nameof(SelectedVerticalValueBox));
            }
        }

        private string _selectedVerticalValueBox;
        public string SelectedVerticalValueBox
        {
            get => _selectedVerticalValueBox;
            set
            {
                if (Regex.IsMatch(value, INTEGER_PART) ||
                    Regex.IsMatch(value, INTEGER_AND_FRACTIONAL_PARTS))
                {
                    double number;
                    if (double.TryParse(value, out number))
                    {
                        if (number < MIN_OFFSET)
                        {
                            SelectedVerticalValue = MIN_OFFSET;
                        }
                        else if (number > MAX_OFFSET)
                        {
                            SelectedVerticalValue = MAX_OFFSET;
                        }
                        else
                        {
                            SelectedVerticalValue = Math.Round(number, DECIMALS);
                        }
                        OnPropertyChanged(nameof(SelectedActualVerticalValue));
                    }
                }
                else
                {
                    _selectedVerticalValueBox = value;
                }
            }
        }

        public double SelectedHorizontalValue
        {
            get => ImageSettings.HorizontalOffset;
            set
            {
                ImageSettings.HorizontalOffset = value;
                _selectedHorizontalValueBox = value.ToString();
                OnPropertyChanged(nameof(SelectedHorizontalValueBox));
            }
        }

        private string _selectedHorizontalValueBox;
        public string SelectedHorizontalValueBox
        {
            get => _selectedHorizontalValueBox;
            set
            {
                if (Regex.IsMatch(value, INTEGER_PART) ||
                    Regex.IsMatch(value, INTEGER_AND_FRACTIONAL_PARTS))
                {
                    double number;
                    if (double.TryParse(value, out number))
                    {
                        if (number < MIN_OFFSET)
                        {
                            SelectedHorizontalValue = MIN_OFFSET;
                        }
                        else if (number > MAX_OFFSET)
                        {
                            SelectedHorizontalValue = MAX_OFFSET;
                        }
                        else
                        {
                            SelectedHorizontalValue = Math.Round(number, DECIMALS);
                        }
                        OnPropertyChanged(nameof(SelectedActualHorizontalValue));
                    }
                }
                else
                {
                    _selectedHorizontalValueBox = value;
                }
            }
        }
        #endregion

        #region IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case nameof(SelectedHorizontalValueBox):
                        if (!Regex.IsMatch(SelectedHorizontalValueBox, INTEGER_PART) &&
                            !Regex.IsMatch(SelectedHorizontalValueBox, INTEGER_AND_FRACTIONAL_PARTS))
                        {
                            error = "error";
                        }
                        break;

                    case nameof(SelectedVerticalValueBox):
                        if (!Regex.IsMatch(SelectedVerticalValueBox, INTEGER_PART) &&
                            !Regex.IsMatch(SelectedVerticalValueBox, INTEGER_AND_FRACTIONAL_PARTS))
                        {
                            error = "error";
                        }
                        break;
                }
                return error;
            }
        }
        #endregion

        private ExecutionSetupVM _executionSetupVM;
        private MathHelper _mathHelper;
        private IImageReader _imageReader;

        private readonly double MIN_OFFSET;
        private readonly double MAX_OFFSET;
        private readonly int DECIMALS;
        private readonly string INTEGER_AND_FRACTIONAL_PARTS;
        private readonly string INTEGER_PART;

        public ImageCenterSelectorVM(ExecutionSetupVM executionSetupVM,
                                     MathHelper mathHelper,
                                     IImageReader imageReader)
        {
            MIN_OFFSET = double.Parse(App.Current.Configuration["min"]);
            MAX_OFFSET = double.Parse(App.Current.Configuration["max"]);
            DECIMALS = int.Parse(App.Current.Configuration["decimals"]);
            INTEGER_AND_FRACTIONAL_PARTS = App.Current.Configuration["integerAndFractionalParts"];
            INTEGER_PART = App.Current.Configuration["integerPart"];

            _executionSetupVM = executionSetupVM;
            ImageSettings = _executionSetupVM.SelectedSettings;
            _mathHelper = mathHelper;
            _imageReader = imageReader;

            if (ImageSettings.Compressed != null)
            {
                Task.Run(() => Bitmap = _imageReader.ReadAsBitmapSource(ImageSettings.Compressed));
            }

            #region события
            CompressedChangedHandler = delegate (object? s, EventArgs e)
            {
                Task.Run(() =>
                {
                    // null если изображение удалили из списка до окончания процесса сжатия
                    if (ImageSettings != null)
                    {
                        Bitmap = _imageReader.ReadAsBitmapSource(ImageSettings.Compressed);
                    }
                });
            };
            ImageSettings.CompressedChanged += CompressedChangedHandler;

            HorizontalOffsetChangedHandler = delegate (object? s, EventArgs e)
            {
                OnPropertyChanged(nameof(SelectedHorizontalValue));
                OnPropertyChanged(nameof(SelectedActualHorizontalValue));
                SelectedHorizontalValueBox = ImageSettings.HorizontalOffset.ToString();
            };
            ImageSettings.HorizontalOffsetChanged += HorizontalOffsetChangedHandler;

            VerticalOffsetChangedHandler = delegate (object? s, EventArgs e)
            {
                OnPropertyChanged(nameof(SelectedVerticalValue));
                OnPropertyChanged(nameof(SelectedActualVerticalValue));
                SelectedVerticalValueBox = ImageSettings.VerticalOffset.ToString();
            };
            ImageSettings.VerticalOffsetChanged += VerticalOffsetChangedHandler;
            #endregion

            SelectedHorizontalValueBox = SelectedHorizontalValue.ToString();
            SelectedVerticalValueBox = SelectedVerticalValue.ToString();

            HandleUnloadedEventCommand = new RelayCommand(HandleUnloadedEvent);

            MoveOffsetLeftCommand = new RelayCommand(() => MoveOffsetLeft());
            MoveOffsetRightCommand = new RelayCommand(() => MoveOffsetRight());
            MoveOffsetUpCommand = new RelayCommand(() => MoveOffsetUp());
            MoveOffsetDownCommand = new RelayCommand(() => MoveOffsetDown());
        }

        #region изменение смещения с помощью кнопок
        public IRelayCommand MoveOffsetLeftCommand { get; }
        public IRelayCommand MoveOffsetRightCommand { get; }
        public IRelayCommand MoveOffsetUpCommand { get; }
        public IRelayCommand MoveOffsetDownCommand { get; }

        public void MoveOffsetLeft()
        {
            var step = (MAX_OFFSET - MIN_OFFSET)/4;
            var newValue = SelectedHorizontalValue - step;

            double result;
            if (newValue > MAX_OFFSET)
            {
                result = newValue - MAX_OFFSET + MIN_OFFSET;
            }
            else if (newValue < MIN_OFFSET)
            {
                result = newValue - MIN_OFFSET + MAX_OFFSET;
            }
            else
            {
                result = newValue;
            }
            SelectedHorizontalValueBox = Math.Round(result, DECIMALS).ToString();
        }

        public void MoveOffsetRight()
        {
            var step = (MAX_OFFSET - MIN_OFFSET) / 4;
            var newValue = SelectedHorizontalValue + step;

            double result;
            if (newValue > MAX_OFFSET)
            {
                result = newValue - MAX_OFFSET + MIN_OFFSET;
            }
            else if (newValue < MIN_OFFSET)
            {
                result = newValue - MIN_OFFSET + MAX_OFFSET;
            }
            else
            {
                result = newValue;
            }
            SelectedHorizontalValueBox = Math.Round(result, DECIMALS).ToString();
        }

        public void MoveOffsetUp()
        {
            var step = (MAX_OFFSET - MIN_OFFSET) / 4;
            var newValue = SelectedVerticalValue - step;

            double result;
            if (newValue > MAX_OFFSET)
            {
                result = newValue - MAX_OFFSET + MIN_OFFSET;
            }
            else if (newValue < MIN_OFFSET)
            {
                result = newValue - MIN_OFFSET + MAX_OFFSET;
            }
            else
            {
                result = newValue;
            }
            SelectedVerticalValueBox = Math.Round(result, DECIMALS).ToString();
        }

        public void MoveOffsetDown()
        {
            var step = (MAX_OFFSET - MIN_OFFSET) / 4;
            var newValue = SelectedVerticalValue + step;

            double result;
            if (newValue > MAX_OFFSET)
            {
                result = newValue - MAX_OFFSET + MIN_OFFSET;
            }
            else if (newValue < MIN_OFFSET)
            {
                result = newValue - MIN_OFFSET + MAX_OFFSET;
            }
            else
            {
                result = newValue;
            }
            SelectedVerticalValueBox = Math.Round(result, DECIMALS).ToString();
        }
        #endregion

        EventHandler CompressedChangedHandler;
        EventHandler HorizontalOffsetChangedHandler;
        EventHandler VerticalOffsetChangedHandler;

        public IRelayCommand HandleUnloadedEventCommand { get; }

        public void HandleUnloadedEvent()
        {
            if (ImageSettings != null)
            {
                ImageSettings.CompressedChanged -= CompressedChangedHandler;
                ImageSettings.HorizontalOffsetChanged -= HorizontalOffsetChangedHandler;
                ImageSettings.VerticalOffsetChanged -= VerticalOffsetChangedHandler;
            }
        }
    }
}
