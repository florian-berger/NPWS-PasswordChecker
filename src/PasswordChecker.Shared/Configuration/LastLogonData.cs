using Newtonsoft.Json;

namespace PasswordChecker.Shared.Configuration
{
    /// <summary>
    ///     Class that stores the last logon data
    /// </summary>
    public class LastLogonData
    {
        /// <summary>
        ///     The last used server address
        /// </summary>
        [JsonProperty(Order = 1)]
        public string? ServerAddress { get; set; }

        /// <summary>
        ///     Name of the last used database
        /// </summary>
        [JsonProperty(Order = 2)]
        public string? DatabaseName { get; set; }

        /// <summary>
        ///     Name of the last used user
        /// </summary>
        [JsonProperty(Order = 3)]
        public string? Username { get; set; }
    }
}
