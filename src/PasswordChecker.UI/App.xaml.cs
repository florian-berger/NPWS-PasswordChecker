using PasswordChecker.Shared.Configuration;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using PasswordChecker.Resources;
using PasswordChecker.Shared.Helpers;
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

            Current.Dispatcher.Invoke(() =>
            {
                var culture = new CultureInfo(Configuration.Language!);
                LanguageHelper.SetLanguage(culture);

                UiThreadHelper.Initialize();
            });

            new MainWindow().Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            
        }
    }
}
