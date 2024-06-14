using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Panoramas_Editor
{
    internal class ImageCenterSelectorVM : ObservableObject, IDataErrorInfo
    {
        private IMathHelper _mathHelper;
        private ExecutionSetupVM _executionSetupVM;

        private const double MIN_OFFSET = -1;
        private const double MAX_OFFSET = 1;

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
            get => _mathHelper.Map(SelectedVerticalValue, MIN_OFFSET, MAX_OFFSET, 0, ActualHeight_);
            set
            {
                SelectedVerticalValue = _mathHelper.Map(value, 0, ActualHeight_, MIN_OFFSET, MAX_OFFSET);
                OnPropertyChanged(nameof(SelectedVerticalValue));
                OnPropertyChanged();
            }
        }

        public double SelectedActualHorizontalValue
        {
            get => _mathHelper.Map(SelectedHorizontalValue, MIN_OFFSET, MAX_OFFSET, 0, ActualWidth_);
            set
            {
                SelectedHorizontalValue = _mathHelper.Map(value, 0, ActualWidth_, MIN_OFFSET, MAX_OFFSET);
                OnPropertyChanged(nameof(SelectedHorizontalValue));
                OnPropertyChanged();
            }
        }
        #endregion

        #region НЕ Actual
        public ImageSettings ImageSettings
        {
            get => _executionSetupVM.SelectedSettings;
        }
        public BitmapImage? Bitmap
        {
            get => ImageSettings.CompressedBitmapImage;
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
                if (Regex.IsMatch(value, _integerPart) ||
                    Regex.IsMatch(value, _integerAndFractionalParts))
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
                            SelectedVerticalValue = Math.Round(number, 2);
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
                if (Regex.IsMatch(value, _integerPart) ||
                    Regex.IsMatch(value, _integerAndFractionalParts))
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
                            SelectedHorizontalValue = Math.Round(number, 2);
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
                        if (!Regex.IsMatch(SelectedHorizontalValueBox, _integerPart) &&
                            !Regex.IsMatch(SelectedHorizontalValueBox, _integerAndFractionalParts))
                        {
                            error = "error";
                        }
                        break;

                    case nameof(SelectedVerticalValueBox):
                        if (!Regex.IsMatch(SelectedVerticalValueBox, _integerPart) &&
                            !Regex.IsMatch(SelectedVerticalValueBox, _integerAndFractionalParts))
                        {
                            error = "error";
                        }
                        break;
                }
                return error;
            }
        }
        #endregion

        private string _decimalSeparator;
        private string _integerAndFractionalParts;
        private string _integerPart;
        
        public ImageCenterSelectorVM(ExecutionSetupVM executionSetupVM, IMathHelper mathHelper)
        {
            // ^[+-]?    - начало строки может начинаться с + или -
            // \d+       - одна или больше цифр
            // [.,' ]{1} - один из возможных разделителей чисел с плавающей точкой
            // \d+$      - в конце строки одна или больше цифр
            _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            _integerAndFractionalParts = @$"^[+-]?\d+[{_decimalSeparator}]{{1}}\d+$";
            _integerPart = @"^[+-]?\d+$";

            _executionSetupVM = executionSetupVM;
            _mathHelper = mathHelper;
            ImageSettings.CompressedBitmapImageChanged += (s, e) => OnPropertyChanged(nameof(Bitmap));

            //var t1 = _mathHelper.Map(50, 0, 100, -100, 100); // 0
            //var t2 = _mathHelper.Map(0, -50, 50, 100, 200); // 150
            //var t3 = _mathHelper.Map(200, 0, 300, 0, 3); // 2
            //var t4 = _mathHelper.Map(4, 0, 3, 0, 300); // 400

            SelectedHorizontalValueBox = SelectedHorizontalValue.ToString();
            SelectedVerticalValueBox = SelectedVerticalValue.ToString();
        }


    }
}
