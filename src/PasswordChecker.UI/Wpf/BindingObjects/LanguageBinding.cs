namespace PasswordChecker.UI.Wpf.BindingObjects
{
    /// <summary>
    ///     Binding object for configuring the language of the software
    /// </summary>
    public class LanguageBinding(string identifier, string displayName)
    {

        /// <summary>
        ///     Identifier of the language
        /// </summary>
        public string Identifier { get; } = identifier;

        /// <summary>
        ///     Name that should be displayed in the UI
        /// </summary>
        public string DisplayName { get; } = displayName;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DisplayName} ({Identifier})";
        }
    }
}
