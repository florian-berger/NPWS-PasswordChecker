using System;
using System.Threading.Tasks;
using PasswordChecker.Shared.Configuration;
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

        #endregion Commands

        #region Private methods

        private async void Run()
        {
            try
            {
                IsCheckRunning = true;

                _ = SaveLastUserData();
                var api = AuthenticationViewModel.InitializeAuthentication(LoginServerAddress, LoginDatabaseName, LoginUserName);

                if (api is { SessionState: PsrSessionState.Connected })
                {
                    try
                    {
                        await RunPasswordAnalysis(api);
                    }
                    finally
                    {
                        await api.AuthenticationManagerV2.Logout();
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO
            }
            finally
            {
                IsCheckRunning = false;
            }
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
            IsCheckRunning = false;
        }

        private bool CanCancel()
        {
            return IsCheckRunning;
        }

        private void UpdateCommandsCanExecute()
        {
            RunCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
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
            await Task.Delay(2000);
        }

        #endregion Private methods
    }
}
