namespace PasswordChecker.Shared.Helpers
{
    /// <summary>
    ///     Helper class for handling JSON file operations
    /// </summary>
    public static class JsonFileHelper
    {
        /// <summary>
        ///     Loading a JSON file from the file system
        /// </summary>
        /// <typeparam name="T">Type the object should have after deserializing</typeparam>
        /// <param name="path">Path where the file is located</param>
        public static async Task<T?> LoadJsonFile<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            try
            {
                var configFileContent = await File.ReadAllTextAsync(path);
                return JsonSerializer.DeserializeObject<T>(configFileContent);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        ///     Saves a object as JSON file
        /// </summary>
        /// <param name="obj">Object that should be serialized</param>
        /// <param name="path">File path where the objects JSON should be saved</param>
        /// <param name="excludeType">True if the objects types should be excluded. Default = False</param>
        public static async Task SaveJsonFile(object obj, string path, bool excludeType = false)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));

            try
            {
                var json = JsonSerializer.SerializeObject(obj, true, excludeType);
                new FileInfo(path).Directory?.Create();

                await File.WriteAllTextAsync(path, json);
            }
            catch
            {
                // ignored
            }
        }
    }
}
