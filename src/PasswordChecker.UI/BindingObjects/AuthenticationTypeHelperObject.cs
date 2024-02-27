using Prism.Mvvm;

namespace PasswordChecker.UI.BindingObjects
{
    public class AuthenticationTypeHelperObject : BindableBase
    {
        public bool IsDynamicAuthentication
        {
            get => _isDynamicAuthentication;
            set
            {
                if (SetProperty(ref _isDynamicAuthentication, value))
                {
                    RaisePropertyChanged(nameof(ShowConfirmButton));
                }
            }
        } private bool _isDynamicAuthentication;

        public bool IsChangePasswordAuthentication
        {
            get => _isChangePasswordAuthentication;
            set
            {
                if (SetProperty(ref _isChangePasswordAuthentication, value))
                {
                    RaisePropertyChanged(nameof(ShowConfirmButton));
                }
            }
        } private bool _isChangePasswordAuthentication;

        public bool IsAuthTypeSelection
        {
            get => _isAuthTypeSelection;
            set
            {
                if (SetProperty(ref _isAuthTypeSelection, value))
                {
                    RaisePropertyChanged(nameof(ShowConfirmButton));
                }
            }
        } private bool _isAuthTypeSelection;

        public bool ShowConfirmButton => IsDynamicAuthentication || IsChangePasswordAuthentication;

        public void Reset()
        {
            IsDynamicAuthentication = false;
            IsChangePasswordAuthentication = false;
            IsAuthTypeSelection = false;
        }
    }
}
