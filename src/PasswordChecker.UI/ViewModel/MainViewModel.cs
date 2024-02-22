using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordChecker.Data;
using PasswordChecker.Shared.Configuration;
using PasswordChecker.UI.Windows;
using Prism.Commands;
using Prism.Mvvm;
using PsrApi;

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

        /// <summary>
        ///     Name of the database the user wants to log in
        /// </summary>
        public string LoginDatabaseName
        {
            get => _loginDatabaseName;
            set
            {
                if (SetProperty(ref _loginDatabaseName, value))
                {
                    UpdateCommandsCanExecute();
                }
            }
        } private string _loginDatabaseName = string.Empty;

        /// <summary>
        ///     Name of the user that should be logged in
        /// </summary>
        public string LoginUserName
        {
            get => _loginUserName;
            set
            {
                if (SetProperty(ref _loginUserName, value))
                {
                    UpdateCommandsCanExecute();
                }
            } 
        } private string _loginUserName = string.Empty;

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
                LoginDatabaseName = App.Configuration.LogonData.DatabaseName ?? "";
                LoginUserName = App.Configuration.LogonData.Username ?? "";
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

        #endregion Commands

        #region Private methods

        private void Run()
        {
            if (IsCheckRunning)
            {
                // Check is already running, can't run parallel
                return;
            }

            _ = RunAsync();
        }

        private bool CanRun()
        {
            return !IsCheckRunning &&
                   !string.IsNullOrWhiteSpace(LoginServerAddress) &&
                   !string.IsNullOrWhiteSpace(LoginDatabaseName) &&
                   !string.IsNullOrWhiteSpace(LoginUserName);
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

                await SaveLastUserData();

                CurrentProgress = new CheckerProgress
                {
                    Step = CheckerStep.ConnectAndLogin
                };

                logonData = new LogonData(LoginServerAddress, LoginDatabaseName, LoginUserName);

                var api = AuthenticationViewModel.InitializeAuthentication(logonData);

                if (api is { SessionState: PsrSessionState.Connected })
                {
                    logonData.UserDisplayName = api.CurrentUser.DataName();

                    try
                    {
                        await RunPasswordAnalysis(api);
                    }
                    finally
                    {
                        CurrentProgress.Step = CheckerStep.Finish;
                        await api.AuthenticationManagerV2.Logout();
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Error Handling
            }
            finally
            {
                reportData = CurrentProgress?.Report;
                ResetExecution();
            }

            if (reportData != null)
            {
                ShowCheckerReport(reportData, logonData);
            }
        }

        private async Task SaveLastUserData()
        {
            App.Configuration.LogonData ??= new LastLogonData();

            App.Configuration.LogonData.ServerAddress = LoginServerAddress;
            App.Configuration.LogonData.DatabaseName = LoginDatabaseName;
            App.Configuration.LogonData.Username = LoginUserName;
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
                new ConnectionInfo(LoginServerAddress, LoginDatabaseName), _cancelTokenSrc.Token);

            await checker.Run();
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
