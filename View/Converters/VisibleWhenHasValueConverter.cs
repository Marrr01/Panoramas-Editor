using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Panoramas_Editor
{
    internal class VisibleWhenHasValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
