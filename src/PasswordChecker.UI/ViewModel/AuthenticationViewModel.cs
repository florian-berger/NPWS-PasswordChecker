using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using PasswordChecker.Shared.Helpers;
using PasswordChecker.UI.BindingObjects;
using PasswordChecker.UI.Windows;
using Prism.Commands;
using Prism.Mvvm;
using PsrApi.Data;
using PsrApi.Internals.AuthFlow;
using PsrApi.Internals.AuthFlow.Data;
using PsrApi.Managers;

namespace PasswordChecker.UI.ViewModel
{
    public class AuthenticationViewModel : BindableBase
    {
        #region Private variables

        private readonly Window _windowInstance;
        private readonly string _databaseName;
        private readonly string _userName;

        private readonly PsrApi.PsrApi? _apiInstance;
        private IAuthenticationFlow? _authFlow;

        private PsrPolicy? _newPasswordPolicy;

        #endregion Private variables

        #region Properties

        /// <summary>
        ///     Helper object for the auth type
        /// </summary>
        public AuthenticationTypeHelperObject AuthTypeHelper
        {
            get => _authTypeHelper;
            set => SetProperty(ref _authTypeHelper, value);
        } private AuthenticationTypeHelperObject _authTypeHelper;

        /// <summary>
        ///     Information if the authentication is working
        /// </summary>
        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty(ref _isWorking, value);
        } private bool _isWorking;

        /// <summary>
        ///     Username Password Secure give us for authentication
        /// </summary>
        public string? LoginUserWelcome
        {
            get => _loginUserWelcome;
            set => SetProperty(ref _loginUserWelcome, value);
        } private string? _loginUserWelcome;

        /// <summary>
        ///     Authentication that should be displayed
        /// </summary>
        public FillableAuthentication? DisplayAuthentication
        {
            get => _displayAuthentication;
            set
            {
                if (SetProperty(ref _displayAuthentication, value))
                {
                    RaisePropertyChanged(nameof(DynamicAuthentication));
                }
            }
        } private FillableAuthentication? _displayAuthentication;

        /// <summary>
        ///     Helper property that casts the <see cref="FillableAuthentication" /> to
        ///     <see cref="DynamicFillableAuthentication" />, just to remove the warnings in the XAML
        /// </summary>
        public DynamicFillableAuthentication? DynamicAuthentication => DisplayAuthentication as DynamicFillableAuthentication;

        /// <summary>
        ///     New password that should be used for your account
        /// </summary>
        public string? NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                {
                    ValidateNewPassword();
                }
            }
        } private string? _newPassword;

        /// <summary>
        ///     Repetition of the new password
        /// </summary>
        public string? NewPasswordRepetition
        {
            get => _newPasswordRepetition;
            set
            {
                if (SetProperty(ref _newPasswordRepetition, value))
                {
                    ValidateNewPassword();
                }
            }
        } private string? _newPasswordRepetition;

        /// <summary>
        ///     Validation result for new passwords
        /// </summary>
        public string? PolicyErrorText
        {
            get => _policyErrorText;
            set => SetProperty(ref _policyErrorText, value);
        } private string? _policyErrorText;

        #endregion Properties

        #region Constructor

        /// <summary>
        ///     Creates an instance of the ViewModel
        /// </summary>
        /// <param name="serverAddress">Address the checker should connect to</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="userName">Login name of the user</param>
        /// <param name="windowInstance">Instance of the window that contains the ViewModel</param>
        public AuthenticationViewModel(string serverAddress, string databaseName, string userName, Window windowInstance)
        {
            _windowInstance = windowInstance;
            _databaseName = databaseName;
            _userName = userName;

            _authTypeHelper = new AuthenticationTypeHelperObject();

            serverAddress = UriHelper.MakePasswordSecureApiFromUri(serverAddress);
            _apiInstance = new PsrApi.PsrApi(serverAddress, new PsrApiOptions
            {
                ClientName = "PsrApi",
                HttpMessageHandlerFactoryCallback = () => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }
            });

            IsWorking = true;
        }

        #endregion Constructor

        #region Commands

        /// <summary>
        ///     Command to cancel the authentication
        /// </summary>
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(Cancel);
        private DelegateCommand? _cancelCommand;

        /// <summary>
        ///     Command to cancel the authentication
        /// </summary>
        public DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(Confirm);
        private DelegateCommand? _confirmCommand;

        /// <summary>
        ///     Command to start the authentication
        /// </summary>
        public DelegateCommand StartAuthenticationCommand => _startAuthenticationCommand ??= new DelegateCommand(StartAuthentication);
        private DelegateCommand? _startAuthenticationCommand;

        #endregion Commands

        #region Public methods

        /// <summary>
        ///     Starts the authentication process
        /// </summary>
        /// <param name="serverAddress">Address the checker should connect to</param>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="userName">Login name of the user</param>
        /// <returns>Instance of the PsrApi if the authentication succeeded. Otherwise null</returns>
        public static PsrApi.PsrApi? InitializeAuthentication(string serverAddress, string databaseName, string userName)
        {
            var window = new AuthenticationWindow();

            var viewModelInstance = new AuthenticationViewModel(serverAddress, databaseName, userName, window);
            window.DataContext = viewModelInstance;
            window.ShowDialog();

            return window.DialogResult == true ? viewModelInstance._apiInstance : null;
        }

        #endregion Public methods

        #region Private methods

        private void Cancel()
        {
            _windowInstance.DialogResult = false;
        }

        private async void Confirm()
        {
            IsWorking = true;

            try
            {
                await AuthenticateNextStep();
            }
            catch (Exception ex)
            {
                // TODO: Error Handling
            }
            finally
            {
                IsWorking = false;
            }
        }

        private async void StartAuthentication()
        {
            if (_apiInstance == null || _authFlow != null)
            {
                return;
            }

            try
            {
                _authFlow = _apiInstance.AuthenticationManagerV2.StartNewAuthentication(_databaseName, _userName);
                await _authFlow.StartLogin();

                AnalyzeNextRequirement();
            }
            catch (Exception ex)
            {
                // TODO: Error Handling
            }
            finally
            {
                IsWorking = false;
            }
        }

        private async Task AuthenticateNextStep()
        {
            if (_authFlow == null)
            {
                throw new InvalidOperationException("Can't continue, no auth flow is set!");
            }

            if (DisplayAuthentication is FillableChangePasswordAuthentication changePasswordAuth)
            {
                changePasswordAuth.NewPassword = NewPassword;
            }

            await _authFlow.Authenticate(DisplayAuthentication);
            if (_authFlow.IsAuthenticated)
            {
                _windowInstance.DialogResult = true;
            }
            else
            {
                var nextRequirement = _authFlow.GetNextRequirement();
#if DEBUG
                nextRequirement?.PossibleRequirements.RemoveAll(r => r is FillablePkiConfiguration);
#endif

                SetNextRequirement(nextRequirement);
            }
        }

        private void AnalyzeNextRequirement()
        {
            if (_authFlow == null)
            {
                throw new NullReferenceException("No authentication flow existing!");
            }

            LoginUserWelcome = _authFlow.GetNameOfUser();

            var nextRequirement = _authFlow.GetNextRequirement();
            SetNextRequirement(nextRequirement);
        }

        private void SetNextRequirement(AuthenticationRequirements? requirement)
        {
            AuthTypeHelper.Reset();
            PolicyErrorText = null;

            if (requirement != null && requirement.PossibleRequirements.Count > 0)
            {
                if (requirement.PossibleRequirements.Count > 1)
                {
                    // TODO: Display selection of requirement
                }
                else
                {
                    // We need to display exactly this requirement
                    DisplayAuthentication = requirement.PossibleRequirements.First();

                    if (DisplayAuthentication is DynamicFillableAuthentication)
                    {
                        AuthTypeHelper.IsDynamicAuthentication = true;
                    }
                    else if (DisplayAuthentication is FillableChangePasswordAuthentication changePasswordAuth)
                    {
                        _newPasswordPolicy = changePasswordAuth.Policy;
                        ValidateNewPassword();

                        AuthTypeHelper.IsChangePasswordAuthentication = true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"The auth type {DisplayAuthentication.AuthType} is not yet implemented.");
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Didn't get any requirement from the server.");
            }
        }

        private void ValidateNewPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                PolicyErrorText = "New password must not be empty";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordRepetition))
            {
                PolicyErrorText = "Please repeat your new password";
                return;
            }

            if (!NewPassword.Equals(NewPasswordRepetition))
            {
                PolicyErrorText = "Repetition of the password is not correct";
                return;
            }

            if (_newPasswordPolicy != null)
            {
                var validationResult = PasswordManager.ValidatePassword(_newPasswordPolicy, NewPassword, [_userName]);
                if (!validationResult.IsValid)
                {
                    PolicyErrorText = string.Join(Environment.NewLine, validationResult.Errors.Select(v => v.ToString()));
                    return;
                }
            }

            PolicyErrorText = null;
        }

        #endregion Private methods
    }
}
