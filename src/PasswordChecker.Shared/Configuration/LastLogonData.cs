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
    }
}
