using Prism.Mvvm;
using PsrApi.Internals.AuthFlow.Data;

namespace PasswordChecker.UI.Wpf.BindingObjects
{
    public class AuthTypeSelectionBinding(string authType, FillableAuthentication authentication) : BindableBase
    {
        #region Properties

        public string AuthName { get; init; } = authType;

        public FillableAuthentication Authentication { get; init; } = authentication;

        #endregion Properties
    }
}
