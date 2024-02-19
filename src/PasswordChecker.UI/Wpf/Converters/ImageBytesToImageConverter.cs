using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PasswordChecker.UI.Wpf.Converters
{
    public class ImageBytesToImageConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringValue && stringValue.Contains("base64,"))
            {
                var b64Part = stringValue.Split("base64,")[1];
                var imgBytes = System.Convert.FromBase64String(b64Part);

                using var stream = new MemoryStream(imgBytes);
                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }

            throw new InvalidOperationException("No valid data information passed");
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
