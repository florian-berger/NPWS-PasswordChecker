namespace PasswordChecker.Data
{
    /// <summary>
    ///     Data that was used for logon
    /// </summary>
    public class LogonData(string serverAddress, string databaseName, string userName)
    {
        #region Properties

        /// <summary>
        ///     Address of the server
        /// </summary>
        public string ServerAddress { get; set; } = serverAddress;

        /// <summary>
        ///     Name of the database
        /// </summary>
        public string DatabaseName { get; set; } = databaseName;

        /// <summary>
        ///     Name of the user
        /// </summary>
        public string UserName { get; set; } = userName;

        /// <summary>
        ///     Display name of the used user
        /// </summary>
        public string? UserDisplayName { get; set; }

        #endregion Properties
    }
}
