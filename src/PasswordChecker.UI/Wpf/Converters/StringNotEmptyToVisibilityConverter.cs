using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PasswordChecker.UI.Wpf.Converters
{
    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueString)
            {
                if (string.IsNullOrWhiteSpace(valueString))
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
