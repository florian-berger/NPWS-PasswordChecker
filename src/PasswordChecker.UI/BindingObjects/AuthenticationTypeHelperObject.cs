using Prism.Mvvm;

namespace PasswordChecker.UI.BindingObjects
{
    public class AuthenticationTypeHelperObject : BindableBase
    {
        public bool IsDynamicAuthentication
        {
            get => _isDynamicAuthentication;
            set => SetProperty(ref _isDynamicAuthentication, value);
        } private bool _isDynamicAuthentication;

        public bool IsChangePasswordAuthentication
        {
            get => _isChangePasswordAuthentication;
            set => SetProperty(ref _isChangePasswordAuthentication, value);
        } private bool _isChangePasswordAuthentication;

        public void Reset()
        {
            IsDynamicAuthentication = false;
            IsChangePasswordAuthentication = false;
        }
    }
}
