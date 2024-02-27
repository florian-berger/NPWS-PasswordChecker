using System;
using System.Globalization;
using System.Windows.Data;
using PasswordChecker.Resources.Language;

namespace PasswordChecker.UI.Wpf.Converters
{
    public class AuthenticatorNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                var authName = AuthenticationResource.ResourceManager.GetString($"AuthType_{stringValue}");
                if (!string.IsNullOrWhiteSpace(authName))
                {
                    return authName;
                }
            }

            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
