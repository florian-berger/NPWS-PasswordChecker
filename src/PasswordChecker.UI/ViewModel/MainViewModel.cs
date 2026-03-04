using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PasswordChecker.Data;
using PasswordChecker.Shared.Configuration;
using PasswordChecker.Shared.Helpers;
using PasswordChecker.UI.Helpers;
using PasswordChecker.UI.Windows;
using Prism.Commands;
using Prism.Mvvm;
using PsrApi;
using PsrApi.Data;

namespace PasswordChecker.UI.ViewModel
{
    /// <summary>
    ///     Logic for the main window
    /// </summary>
    internal class MainViewModel : BindableBase
    {
        #region Private variables

        private CancellationTokenSource? _cancelTokenSrc;

        #endregion Private variables

        #region Properties

        /// <summary>
        ///     Information if the check is currently running
        /// </summary>
        public bool IsCheckRunning
        {
            get => _isCheckRunning;
            set
            {
                if (SetProperty(ref _isCheckRunning, value))
                {
                    UpdateCommandsCanExecute();
                }
            }
        } private bool _isCheckRunning;

        /// <summary>
        ///     Address of the NPWS server (mainly: URI of the WebClient)
        /// </summary>
        public string LoginServerAddress
        {
            get => _loginServerAddress;
            set
            {
                if (SetProperty(ref _loginServerAddress, value))
                {
                    UpdateCommandsCanExecute();
                }
            }
        } private string _loginServerAddress = string.Empty;

        public string LoginApiKey
        {
            get => _loginApiKey;
            set
            {
                if (SetProperty(ref _loginApiKey, value))
                {
                    UpdateCommandsCanExecute();
                }
            }
        } private string _loginApiKey = string.Empty;

        public CheckerProgress? CurrentProgress
        {
            get => _currentProgress;
            private set => SetProperty(ref _currentProgress, value);
        } private CheckerProgress? _currentProgress;

        #endregion Properties

        #region Constructor

        public MainViewModel()
        {
            if (App.Configuration.LogonData != null)
            {
                LoginServerAddress = App.Configuration.LogonData.ServerAddress ?? "";
            }
        }

        #endregion Constructor

        #region Commands

        /// <summary>
        ///     Command to start the check
        /// </summary>
        public DelegateCommand RunCommand => _runCommand ??= new DelegateCommand(Run, CanRun);
        private DelegateCommand? _runCommand;

        /// <summary>
        ///     Command to cancel the current execution
        /// </summary>
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(Cancel, CanCancel);
        private DelegateCommand? _cancelCommand;

        /// <summary>
        ///     Command to open the settings
        /// </summary>
        public DelegateCommand SettingsCommand => _settingsCommand ??= new DelegateCommand(Settings);
        private DelegateCommand? _settingsCommand;

        /// <summary>
        ///     Command to open the third party licenses
        /// </summary>
        public DelegateCommand ShowThirdPartyLicensesCommand => _showThirdPartyLicensesCommand ??= new DelegateCommand(ShowThirdPartyLicenses);
        private DelegateCommand? _showThirdPartyLicensesCommand;

        #endregion Commands

        #region Private methods

        private void Run()
        {
            if (IsCheckRunning)
            {
                // Check is already running, can't run parallel
                return;
            }

            // start the entire workflow on a thread‑pool thread so continuations never capture the UI context
            _ = Task.Run(() => RunAsync());
        }

        private bool CanRun()
        {
            return !IsCheckRunning &&
                   !string.IsNullOrWhiteSpace(LoginServerAddress) &&
                   !string.IsNullOrWhiteSpace(LoginApiKey);
        }

        private void Cancel()
        {
            if (!CancelCommand.CanExecute())
            {
                Console.WriteLine(@"Check is not running - cancel!");
                return;
            }

            _cancelTokenSrc?.Cancel();
        }

        private bool CanCancel()
        {
            return IsCheckRunning;
        }

        private void Settings()
        {
            SettingsViewModel.OpenSettingsWindow();
        }

        private void ShowThirdPartyLicenses()
        {
            ThirdPartyLicensesViewModel.OpenLicensesWindow();
        }

        private void UpdateCommandsCanExecute()
        {
            RunCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private async Task RunAsync()
        {
            ReportData? reportData;
            LogonData? logonData = default;

            try
            {
                IsCheckRunning = true;

                await SaveLastUserData().ConfigureAwait(false);

                // update progress on UI thread explicitly
                UiThreadHelper.RunOnUiThread(() =>
                {
                    CurrentProgress = new CheckerProgress
                    {
                        Step = CheckerStep.ConnectAndLogin
                    };
                });

                logonData = new LogonData(LoginServerAddress);

                var serverAddress = UriHelper.MakePasswordSecureApiFromUri(logonData.ServerAddress);
                var api = new PsrApi.PsrApi(serverAddress, new PsrApiOptions
                {
                    ClientName = "PsrApi", // Hack, so the API is allowed to be newer than the server
                    HttpMessageHandlerFactoryCallback = () => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }
                });
                await api.AuthenticationManagerV2.LoginWithApiKey(LoginApiKey).ConfigureAwait(false);

                if (api is { SessionState: PsrSessionState.Connected })
                {
                    logonData.DatabaseName = api.ApiKeyManager.GetDatabaseName(LoginApiKey);
                    logonData.ApiKeyAccessRights = string.Join(", ", api.ApiKeyManager.GetAccessRights(LoginApiKey));
                    logonData.ApiKeyAccessScopes = string.Join(", ", api.ApiKeyManager.GetAccessScopes(LoginApiKey));
                    logonData.ApiKeyExpirationDate = api.ApiKeyManager.GetExpirationDateUtc(LoginApiKey);
                    logonData.UserName = api.CurrentUser.UserName;
                    logonData.UserDisplayName = api.CurrentUser.DataName();

                    try
                    {
                        await RunPasswordAnalysis(api).ConfigureAwait(false);
                    }
                    finally
                    {
                        UiThreadHelper.RunOnUiThread(() => CurrentProgress.Step = CheckerStep.Finish);
                        await api.AuthenticationManagerV2.Logout().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                UiThreadHelper.RunOnUiThread(() => CustomMessageBox.ShowErrorDialog(ex));
            }
            finally
            {
                reportData = CurrentProgress?.Report;
                ResetExecution();
            }

            if (reportData != null)
            {
                UiThreadHelper.RunOnUiThread(() => ShowCheckerReport(reportData, logonData));
            }
        }

        private async Task SaveLastUserData()
        {
            App.Configuration.LogonData ??= new LastLogonData();

            App.Configuration.LogonData.ServerAddress = LoginServerAddress;
            await App.Configuration.SaveConfig();
        }

        private async Task RunPasswordAnalysis(PsrApi.PsrApi apiInstance)
        {
            if (CurrentProgress == null)
            {
                throw new NullReferenceException("No progress is set!");
            }

            _cancelTokenSrc = new CancellationTokenSource();

            var ignoredFields = App.Configuration.IgnoredFieldNames ?? [];
            var checker = new Checker(apiInstance, CurrentProgress, ignoredFields,
                new ConnectionInfo(LoginServerAddress, apiInstance.CurrentUser.DataName()), _cancelTokenSrc.Token);

            await checker.Run().ConfigureAwait(false);
        }

        private void ResetExecution()
        {
            IsCheckRunning = false;
            CurrentProgress = null;
            CancelCommand.Execute();
        }

        private void ShowCheckerReport(ReportData data, LogonData? logonData)
        {
            new ReportWindow(data, logonData).ShowDialog();
        }

        #endregion Private methods
    }
}
