using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PasswordChecker.UI.Wpf.Converters
{
    internal class IntegerHigherThanNullToVisibilityConverter : IValueConverter
    {
        #region Properties

        /// <summary>
        /// Set to true if you want your item Hidden instead of Collapsed
        /// </summary>
        public bool HiddenInsteadOfCollapsed { get; set; } = false;

        /// <summary>
        /// Set to true if you want to inverse the converter
        /// </summary>
        public bool Inverse { get; set; } = false;

        #endregion Properties

        #region IValueConverter

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int intVal))
            {
                throw new ArgumentException(@"Parameter must be a boolean to be used with this converter.", nameof(value));
            }

            if (Inverse)
            {
                return intVal > 0
                    ? (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed)
                    : Visibility.Visible;
            }

            return intVal > 0
                ? Visibility.Visible
                : (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion IValueConverter
    }
}
