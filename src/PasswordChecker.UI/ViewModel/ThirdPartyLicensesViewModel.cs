using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using PasswordChecker.Resources.Language;
using PasswordChecker.UI.BindingObjects;
using PasswordChecker.UI.Windows;
#if !DEBUG
using PasswordChecker.Resources.Language;
#endif

namespace PasswordChecker.UI.ViewModel
{
    /// <summary>
    ///     ViewModel for the third party licenses
    /// </summary>
    public class ThirdPartyLicensesViewModel : BindableBase
    {
        #region Properties

        /// <summary>
        ///     All licenses that are used
        /// </summary>
        public IEnumerable<ThirdPartyLicenseBinding> Licenses => _licenses ??= BuildLicenses();
        private static IEnumerable<ThirdPartyLicenseBinding>? _licenses;

        /// <summary>
        ///     Selected license object
        /// </summary>
        public ThirdPartyLicenseBinding? SelectedLicense
        {
            get => _selectedLicense;
            set => SetProperty(ref _selectedLicense, value);
        } private ThirdPartyLicenseBinding? _selectedLicense;

        #endregion Properties

        #region Public methods

        public static void OpenLicensesWindow()
        {
            new ThirdPartyLicensesWindow().Show();
        }

        #endregion Public methods

        #region Private methods

        private IEnumerable<ThirdPartyLicenseBinding> BuildLicenses()
        {
            return new List<ThirdPartyLicenseBinding>
            {
                new()
                {
                    Name = "FontAwesome.Sharp",
                    Author = "Awesome Incremented and Contributors",
                    Version = new Version(6, 6, 0),
                    Uri = "https://www.nuget.org/packages/FontAwesome.Sharp/6.3.0",
                    LicenseType = "Apache-2.0",
                    LicenseText = GetLicenseTextFromResources("FontAwesomeSharp")
                },
                new()
                {
                    Name = "Microsoft.Extensions.Hosting",
                    Author = "Microsoft",
                    Version = new Version(10, 0, 1),
                    Uri = "https://www.nuget.org/packages/Microsoft.Extensions.Hosting/3.1.8",
                    LicenseType = "Apache-2.0",
                    LicenseText = GetLicenseTextFromResources("MicrosoftExtensionsHosting")
                },
                new()
                {
                    Name = "Microsoft.Xaml.Behaviors.Wpf",
                    Author = "Microsoft",
                    Version = new Version(1, 1, 135),
                    Uri = "https://github.com/Microsoft/XamlBehaviorsWpf",
                    LicenseType = "MIT",
                    LicenseText = GetLicenseTextFromResources("MicrosoftXamlBehaviorsWpf")
                },
                new()
                {
                    Name = "Newtonsoft.Json",
                    Author = "James Newton-King",
                    Version = new Version(13, 0, 4),
                    Uri = "https://www.newtonsoft.com/json",
                    LicenseType = "MIT",
                    LicenseText = GetLicenseTextFromResources("NewtonsoftJson")
                },
                new()
                {
                    Name = "Prism.Core",
                    Author = "Brian Lagunas, Dan Siegel",
                    Version = new Version(9, 0, 537),
                    Uri = "https://www.nuget.org/packages/Prism.Core/8.1.97",
                    LicenseType = "MIT",
                    LicenseText = GetLicenseTextFromResources("PrismCore")
                },
                new()
                {
                    Name = "Syncfusion Community",
                    Author = "Syncfusion, Inc.",
                    Version = new Version(24, 2, 6),
                    Uri = "https://www.syncfusion.com/products/communitylicense",
                    LicenseType = ThirdPartyLicensesResource.LicenseTypeCustom,
                    LicenseText = "https://www.syncfusion.com/content/downloads/syncfusion_license.pdf",
                    IsLicenseTextUri = true
                },
            }.OrderBy(l => l.Name);
        }

        private string GetLicenseTextFromResources(string name)
        {
            var resName = $"PasswordChecker.Resources.Licenses.{name}.txt";

            var assembly = Assembly.GetAssembly(typeof(Resources.LanguageHelper));
            using var stream = assembly?.GetManifestResourceStream(resName);
            if (stream != null)
            {
                using var sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }

#if DEBUG
            throw new InvalidOperationException($"Missing license file for {name}!");
#else
            return ThirdPartyLicensesResource.LicenseNotFound;
#endif
        }

        #endregion Private methods
    }
}
