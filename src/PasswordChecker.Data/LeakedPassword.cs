namespace PasswordChecker.Data
{
    /// <summary>
    ///     Information of a single breached password
    /// </summary>
    public class LeakedPassword(string identifier, int count)
    {
        /// <summary>
        ///     Identifier of the password
        /// </summary>
        public string? Identifier { get; init; } = identifier;

        /// <summary>
        ///     Number how often the password was breached
        /// </summary>
        public int Count { get; init; } = count;
    }
}
