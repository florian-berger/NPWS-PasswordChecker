using PasswordChecker.Shared.Configuration;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using PasswordChecker.UI.Windows;

namespace PasswordChecker.UI
{
    public partial class App
    {
        #region Properties

        /// <summary>
        ///     Configuration of the app
        /// </summary>
        internal static CheckerConfig Configuration { get; private set; } = new();

        #endregion Properties

        #region Constructor

        public App()
        {
#if DEBUG
            var licenseKey = Environment.GetEnvironmentVariable("PasswordChecker_SyncFusion_License", EnvironmentVariableTarget.User);
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
#else
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("%LICENSE_KEY%");
#endif
        }

        #endregion Constructor

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            Configuration = await CheckerConfig.LoadConfig();

            var culture = new CultureInfo(Configuration.Language!);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            new MainWindow().Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
        }
    }
}
