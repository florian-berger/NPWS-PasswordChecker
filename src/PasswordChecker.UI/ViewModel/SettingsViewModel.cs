using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using PasswordChecker.Shared.Configuration;
using PasswordChecker.UI.Windows;
using PasswordChecker.UI.Wpf.BindingObjects;
using Prism.Commands;
using Prism.Mvvm;

namespace PasswordChecker.UI.ViewModel
{
    public class SettingsViewModel : BindableBase
    {
        #region Private variables

        private readonly string _currentLanguage;
        private readonly Window _windowInstance;

        #endregion Private variables

        #region Constructor

        public SettingsViewModel(Window window)
        {
            _windowInstance = window;

            _currentLanguage = CultureInfo.DefaultThreadCurrentUICulture?.IetfLanguageTag ?? CheckerConfig.SupportedLanguages.First();
            _selectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Identifier == App.Configuration.Language) ?? AvailableLanguages.First();

            _ignoredFieldsConfig = string.Join(Environment.NewLine, App.Configuration.IgnoredFieldNames ?? []);
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        ///     Selected language
        /// </summary>
        public LanguageBinding SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    RaisePropertyChanged(nameof(ShowLanguageChangedHint));
                }
            }
        } private LanguageBinding _selectedLanguage;

        /// <summary>
        ///     List of all available languages
        /// </summary>
        public List<LanguageBinding> AvailableLanguages
        {
            get
            {
                if (_availableLanguages != null)
                {
                    return _availableLanguages;
                }

                return _availableLanguages = CheckerConfig.SupportedLanguages
                    .Select(l => new LanguageBinding(l, new CultureInfo(l).DisplayName)).ToList();
            }
        } private List<LanguageBinding>? _availableLanguages;

        /// <summary>
        ///     Configuration for which fields should be ignored during checks
        /// </summary>
        public string IgnoredFieldsConfig
        {
            get => _ignoredFieldsConfig;
            set => SetProperty(ref _ignoredFieldsConfig, value);
        } private string _ignoredFieldsConfig;

        /// <summary>
        ///     Information if the hint about a necessary restart should be displayed
        /// </summary>
        public bool ShowLanguageChangedHint => _currentLanguage != SelectedLanguage.Identifier;

        #endregion Properties

        #region Commands

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(Save);
        private DelegateCommand? _saveCommand;

        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(Cancel);
        private DelegateCommand? _cancelCommand;

        #endregion Commands

        #region Public methods

        public static void OpenSettingsWindow()
        {
            var window = new SettingsWindow();

            var viewModelInstance = new SettingsViewModel(window);
            window.DataContext = viewModelInstance;
            window.ShowDialog();
        }

        #endregion Public methods

        #region Private methods

        private async void Save()
        {
            App.Configuration.Language = SelectedLanguage.Identifier;
            App.Configuration.IgnoredFieldNames = IgnoredFieldsConfig.Split(new[] { '\v', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            await App.Configuration.SaveConfig();

            _windowInstance.DialogResult = true;
        }

        private void Cancel()
        {
            _windowInstance.DialogResult = false;
        }

        #endregion Private methods
    }
}
