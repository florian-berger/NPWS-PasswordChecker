using System.Globalization;
using PasswordChecker.Resources.Language;

namespace PasswordChecker.Resources
{
    public static class LanguageHelper
    {
        public static void SetLanguage(CultureInfo culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            AuthenticationResource.Culture = culture;
            GlobalResource.Culture = culture;
            MainResource.Culture = culture;
            ReportResource.Culture = culture;
            SettingsResource.Culture = culture;
        }
    }
}
