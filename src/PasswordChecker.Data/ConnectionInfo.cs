namespace PasswordChecker.Data
{
    /// <summary>
    ///     Information that was used for connection
    /// </summary>
    public class ConnectionInfo(string serverAddress, string database)
    {
        /// <summary>
        ///     User that was logged in
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        ///     Address of the server
        /// </summary>
        public string ServerAddress { get; init; } = serverAddress;

        /// <summary>
        ///     Database the connection was established to
        /// </summary>
        public string Database { get; init; } = database;
    }
}
