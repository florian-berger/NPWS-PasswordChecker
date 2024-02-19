using System.Windows;
using System.Windows.Controls;
using PsrApi.Data.Authentication.Enums;
using PsrApi.Internals.AuthFlow.Data;

namespace PasswordChecker.UI.Wpf.TemplateSelectors
{
    public class AuthenticationFieldTemplateSelector : DataTemplateSelector
    {
        #region Properties

        /// <summary>
        ///     Template for password fields
        /// </summary>
        public DataTemplate? PasswordFieldTemplate { get; set; }

        /// <summary>
        ///     Template for PIN fields
        /// </summary>
        public DataTemplate? PinFieldTemplate { get; set; }

        /// <summary>
        ///     Template for token fields
        /// </summary>
        public DataTemplate? TokenFieldTemplate { get; set; }

        /// <summary>
        ///     Template for username fields
        /// </summary>
        public DataTemplate? UsernameFieldTemplate { get; set; }

        /// <summary>
        ///     Template for QR code fields
        /// </summary>
        public DataTemplate? QrCodeFieldTemplate { get; set; }

        /// <summary>
        ///     Template for token usernames
        /// </summary>
        public DataTemplate? TokenUsernameFieldTemplate { get; set; }

        /// <summary>
        ///     Template for field types that are not supported
        /// </summary>
        public DataTemplate? FieldTypeNotSupportedTemplate { get; set; }

        #endregion Properties

        #region Public methods

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object? item, DependencyObject container)
        {
            var template = default(DataTemplate?);

            if (item is AuthenticationField authField)
            {
                switch (authField.FieldType)
                {
                    case AuthenticationFieldType.Password:
                        template = PasswordFieldTemplate;
                        break;
                    case AuthenticationFieldType.Pin:
                        template = PinFieldTemplate;
                        break;
                    case AuthenticationFieldType.Token:
                        template = TokenFieldTemplate;
                        break;
                    case AuthenticationFieldType.Username:
                        template = UsernameFieldTemplate;
                        break;
                    case AuthenticationFieldType.QrCode:
                        template = QrCodeFieldTemplate;
                        break;
                    case AuthenticationFieldType.TokenUsername:
                        template = TokenUsernameFieldTemplate;
                        break;
                }
            }

            return template ?? FieldTypeNotSupportedTemplate ?? new DataTemplate();
        }

        #endregion Public methods
    }
}
