namespace PasswordChecker.Data
{
    /// <summary>
    ///     Summary of password qualities
    /// </summary>
    public class QualityStore
    {
        /// <summary>
        ///     List of weak passwords
        /// </summary>
        public required List<string?> WeakPasswords { get; init; }

        /// <summary>
        ///     List of good passwords
        /// </summary>
        public required List<string?> GoodPasswords { get; init; }

        /// <summary>
        ///     List of strong passwords
        /// </summary>
        public required List<string?> StrongPasswords { get; init; }
    }
}
