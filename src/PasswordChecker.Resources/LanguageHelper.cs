using System.Diagnostics;
using System.Globalization;
using System.Windows.Markup;
using System.Windows;
using PasswordChecker.Resources.Language;

namespace PasswordChecker.Resources
{
    public static class LanguageHelper
    {
        #region Private variables

        private static CultureInfo? _currentCulture;

        #endregion Private variables

        #region Public methods

        public static void SetLanguage(CultureInfo culture)
        {
            _currentCulture = culture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            AuthenticationResource.Culture = culture;
            GlobalResource.Culture = culture;
            MainResource.Culture = culture;
            PdfFileResource.Culture = culture;
            ReportResource.Culture = culture;
            SettingsResource.Culture = culture;
        }

        public static CultureInfo GetCurrentCulture()
        {
            return _currentCulture ?? CultureInfo.CurrentUICulture;
        }

        #endregion Public methods
    }
}
