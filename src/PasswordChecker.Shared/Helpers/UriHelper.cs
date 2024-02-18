namespace PasswordChecker.Shared.Helpers
{
    /// <summary>
    ///     Provides helper methods for URIs
    /// </summary>
    public static class UriHelper
    {
        public static string MakePasswordSecureApiFromUri(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var domain = input.TrimEnd('/').Replace("http://", "").Replace("https://", "").Replace("/api", "");
            return domain.Contains(":11016") ? $"https://{domain}" : $"https://{domain}/api";
        }
    }
}
