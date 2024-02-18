using Newtonsoft.Json;
using PasswordChecker.Shared.Helpers;

namespace PasswordChecker.Shared.Configuration
{
    /// <summary>
    ///     Configuration for the PasswordChecker
    /// </summary>
    public class CheckerConfig
    {
        #region Properties

        /// <summary>
        ///     Array of all supported language identifiers
        /// </summary>
        [JsonIgnore]
        public static readonly string[] SupportedLanguages = [ "en-US", "de-DE" ];

        /// <summary>
        ///     Path to the configuration file
        /// </summary>
        [JsonIgnore]
        private const string ConfigFileName = "%appdata%/Florian Berger/NPWS-PasswordChecker.json";

        /// <summary>
        ///     Language the UI should be displayed (Supported values: See <see cref="SupportedLanguages" />)
        /// </summary>
        [JsonProperty(Order = 1)]
        public string? Language { get; set; }

        /// <summary>
        ///     List of field names that should be ignored
        /// </summary>
        [JsonProperty(Order = 2)]
        public List<string>? IgnoredFieldNames { get; set; }

        /// <summary>
        ///     Last used logon data
        /// </summary>
        [JsonProperty(Order = 3)]
        public LastLogonData? LogonData { get; set; }

        #endregion Properties

        #region Public methods

        /// <summary>
        ///     Loads the currently stored configuration (if not existing, the default one)
        /// </summary>
        public static async Task<CheckerConfig> LoadConfig()
        {
            var fileName = Environment.ExpandEnvironmentVariables(ConfigFileName);

            if (!File.Exists(fileName))
            {
                return await CreateAndSaveDefaultConfig();
            }

            var existingConfig = await JsonFileHelper.LoadJsonFile<CheckerConfig>(fileName);
            if (existingConfig == null)
            {
                return await CreateAndSaveDefaultConfig();
            }

            if (existingConfig.Language == null || SupportedLanguages.All(l => l != existingConfig.Language))
            {
                existingConfig.Language = SupportedLanguages.First();
            }

            if (existingConfig.LogonData == null)
            {
                existingConfig.LogonData = new LastLogonData();
            }

            return existingConfig;
        }

        /// <summary>
        ///     Saves the configuration to file system
        /// </summary>
        public async Task SaveConfig()
        {
            var fileName = Environment.ExpandEnvironmentVariables(ConfigFileName);
            await JsonFileHelper.SaveJsonFile(this, fileName);
        }

        #endregion Public methods

        #region Private methods

        private static async Task<CheckerConfig> CreateAndSaveDefaultConfig()
        {
            var defaultConfig = new CheckerConfig
            {
                Language = SupportedLanguages.First(),
                IgnoredFieldNames = [ "^(.*)PIN(.*)$" ]
            };

            await defaultConfig.SaveConfig();
            return defaultConfig;
        }

        #endregion Private methods
    }
}
