using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using PasswordChecker.Data;
using PasswordChecker.Resources.Language;
using PasswordChecker.Shared.Helpers;
using PasswordChecker.UI.BindingObjects;
using PasswordChecker.UI.Enums;
using PasswordChecker.UI.Helpers;
using PasswordChecker.UI.Windows;
using PasswordChecker.UI.Wpf.BindingObjects;
using Prism.Commands;
using Prism.Mvvm;
using PsrApi.Data;
using PsrApi.Data.Authentication.AdditionalObjects;
using PsrApi.Internals.AuthFlow;
using PsrApi.Internals.AuthFlow.Data;
using PsrApi.Managers;
using JsonSerializer = PasswordChecker.Shared.Helpers.JsonSerializer;

namespace PasswordChecker.UI.ViewModel
{
    public class AuthenticationViewModel : BindableBase, IDisposable
    {
        #region Private variables

        private readonly Window _windowInstance;
        private readonly string _databaseName;
        private readonly string _userName;

        private readonly PsrApi.PsrApi? _apiInstance;
        private IAuthenticationFlow? _authFlow;

        private PsrPolicy? _newPasswordPolicy;
        private readonly Timer _loginLockTimer;

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

        /// <summary>
        ///     Possible selections for the user
        /// </summary>
        public List<AuthTypeSelectionBinding>? AuthTypesForSelection
        {
            get => _authTypesForSelection;
            set => SetProperty(ref _authTypesForSelection, value);
        } private List<AuthTypeSelectionBinding>? _authTypesForSelection;

        public LoginLock? LockInformation
        {
            get => _lockInformation;
            set
            {
                if (SetProperty(ref _lockInformation, value))
                {
                    ConfirmCommand.RaiseCanExecuteChanged();
                }

                if (_lockInformation != null)
                {
                    _loginLockTimer.Start();
                }
                else
                {
                    _loginLockTimer.Stop();
                }
            }
        } private LoginLock? _lockInformation;

        public string? LoginLockedText
        {
            get => _loginLockedText;
            set => SetProperty(ref _loginLockedText, value);
        } private string? _loginLockedText;

        #endregion Properties

        #region Constructor

        /// <summary>
        ///     Creates an instance of the ViewModel
        /// </summary>
        /// <param name="logonData">Data that should be used for logon</param>
        /// <param name="windowInstance">Instance of the window that contains the ViewModel</param>
        public AuthenticationViewModel(LogonData logonData, Window windowInstance)
        {
            _windowInstance = windowInstance;
            _databaseName = logonData.DatabaseName;
            _userName = logonData.UserName;

            _authTypeHelper = new AuthenticationTypeHelperObject();
            _loginLockTimer = new Timer(1000)
            {
                Enabled = false,
                AutoReset = true
            };
            _loginLockTimer.Elapsed += LoginLockTimerOnElapsed;

            var serverAddress = UriHelper.MakePasswordSecureApiFromUri(logonData.ServerAddress);
            _apiInstance = new PsrApi.PsrApi(serverAddress, new PsrApiOptions
            {
                ClientName = "PsrApi", // Hack, so the API is allowed to be newer than the server
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
        public DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(Confirm, CanConfirm);
        private DelegateCommand? _confirmCommand;

        /// <summary>
        ///     Command to start the authentication
        /// </summary>
        public DelegateCommand StartAuthenticationCommand =>
            _startAuthenticationCommand ??= new DelegateCommand(StartAuthentication);
        private DelegateCommand? _startAuthenticationCommand;

        /// <summary>
        ///     Command to let the user select an authenticator
        /// </summary>
        public DelegateCommand<AuthTypeSelectionBinding> SelectAuthenticatorCommand => _selectAuthenticatorCommand ??=
            new DelegateCommand<AuthTypeSelectionBinding>(SelectAuthenticator);
        private DelegateCommand<AuthTypeSelectionBinding>? _selectAuthenticatorCommand;

        #endregion Commands

        #region Public methods

        /// <summary>
        ///     Starts the authentication process
        /// </summary>
        /// <param name="logonData">Data that should be used for logon</param>
        /// <returns>Instance of the PsrApi if the authentication succeeded. Otherwise null</returns>
        public static PsrApi.PsrApi? InitializeAuthentication(LogonData logonData)
        {
            var window = new AuthenticationWindow();

            var viewModelInstance = new AuthenticationViewModel(logonData, window);
            window.DataContext = viewModelInstance;
            window.ShowDialog();

            return window.DialogResult == true ? viewModelInstance._apiInstance : null;
        }

        #endregion Public methods

        #region Private methods

        private void Cancel()
        {
            _loginLockTimer.Stop();
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
                HandleLoginException(ex);
            }
            finally
            {
                IsWorking = false;
            }
        }

        private bool CanConfirm()
        {
            return LockInformation == null;
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
                HandleLoginException(ex);

                // The user is locked, we can't show any requirement.
                if (LockInformation != null)
                {
                    UiThreadHelper.RunOnUiThread(() =>
                    {
                        Cancel();

                        CustomMessageBoxWindow.ShowDialog(LoginLockedText ?? AuthenticationResource.LoginIsLocked,
                            GlobalResource.Error, CustomMessageBoxButtons.Ok, CustomMessageBoxImage.Error);
                    });
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void SelectAuthenticator(AuthTypeSelectionBinding authenticator)
        {
            ArgumentNullException.ThrowIfNull(authenticator);

            SetNextRequirement([authenticator.Authentication]);
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
                FilterUnsupportedAuthentications(nextRequirement);
                SetNextRequirement(nextRequirement.PossibleRequirements);
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
            SetNextRequirement(nextRequirement.PossibleRequirements);
        }

        private void SetNextRequirement(List<FillableAuthentication>? possibleRequirements)
        {
            AuthTypeHelper.Reset();
            PolicyErrorText = null;

            if (possibleRequirements != null && possibleRequirements.Count > 0)
            {
                if (possibleRequirements.Count > 1)
                {
                    AuthTypesForSelection = possibleRequirements.Select(r => new AuthTypeSelectionBinding(r.AuthType, r)).ToList();
                    AuthTypeHelper.IsAuthTypeSelection = true;
                }
                else
                {
                    // We need to display exactly this requirement
                    DisplayAuthentication = possibleRequirements.First();

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
                throw new InvalidOperationException("Didn't get any supported requirement from the server.");
            }
        }

        private void ValidateNewPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                PolicyErrorText = AuthenticationResource.ChangePasswordNoPassword;
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordRepetition))
            {
                PolicyErrorText = AuthenticationResource.ChangePasswordRepeatPassword;
                return;
            }

            if (!NewPassword.Equals(NewPasswordRepetition))
            {
                PolicyErrorText = AuthenticationResource.NewPasswordRepetitionNotCorrect;
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

        private void FilterUnsupportedAuthentications(AuthenticationRequirements? requirement)
        {
            if (requirement?.PossibleRequirements == null || requirement.PossibleRequirements.Count < 1)
            {
                return;
            }

            requirement.PossibleRequirements.RemoveAll(a => a is FillablePkiConfiguration);
            requirement.PossibleRequirements.RemoveAll(a => a is FillablePkiCredential);
            requirement.PossibleRequirements.RemoveAll(a => a is FillableOdicCredential);
            requirement.PossibleRequirements.RemoveAll(a => a is FillableSmartCardCredential);
        }

        private void HandleLoginException(Exception exception)
        {
            // If the exception is a login lock, the details of it are serialized in the message.
            // So we try to deserialize it and handle it properly then
            if (!string.IsNullOrWhiteSpace(exception.InnerException?.Message))
            {
                try
                {
                    var loginLock = JsonSerializer.DeserializeObject<LoginLock>(exception.InnerException.Message);
                    if (loginLock != null)
                    {
                        HandleLoginLock(loginLock);
                        return;
                    }
                }
                catch (JsonSerializationException)
                {
                    // No login lock - handle it another way
                }
            }

            HandleOtherException(exception);
        }

        private void HandleLoginLock(LoginLock loginLock)
        {
            ArgumentNullException.ThrowIfNull(loginLock);

            UiThreadHelper.RunOnUiThread(() =>
            {
                LockInformation = loginLock;
                SetLoginLockedText();
            });
        }

        private void HandleOtherException(Exception exception)
        {
            UiThreadHelper.RunOnUiThread(() =>
            {
                CustomMessageBoxWindow.ShowDialog(ExceptionHelper.GetExceptionText(exception), GlobalResource.Error,
                    CustomMessageBoxButtons.Ok, CustomMessageBoxImage.Error, _windowInstance);
            });
        }

        private void LoginLockTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            UiThreadHelper.RunOnUiThread(SetLoginLockedText);
        }

        private void SetLoginLockedText()
        {
            var now = DateTime.UtcNow;
            if (LockInformation == null || LockInformation.LockedUntilUtc < now)
            {
                LockInformation = null;
                LoginLockedText = null;
                return;
            }

            var timeDifference = LockInformation.LockedUntilUtc - now;
            if (timeDifference.Minutes > 0)
            {
                var localTime = LockInformation.LockedUntilUtc.ToLocalTime();
                LoginLockedText = string.Format(AuthenticationResource.LoginLockedUntil, LockInformation.LoginTry,
                    localTime.ToString("G"));
            }
            else
            {
                LoginLockedText = string.Format(AuthenticationResource.LoginLockedForSeconds, LockInformation.LoginTry,
                    Math.Max(1, timeDifference.Seconds));
            }
        }

        #endregion Private methods

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _loginLockTimer.Dispose();
        }

        #endregion IDisposable
    }
}
