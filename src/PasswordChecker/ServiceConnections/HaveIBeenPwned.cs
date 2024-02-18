using System.Net;
using System.Reflection;
using PasswordChecker.Helpers;

namespace PasswordChecker.ServiceConnections
{
    /// <summary>
    ///     Providing access to the API of Have I Been Pwned: https://haveibeenpwned.com/API/v3#SearchingPwnedPasswordsByRange
    /// </summary>
    internal class HaveIBeenPwned
    {
        #region Constants

        private const string ApiBaseUri = "https://api.pwnedpasswords.com/range";

        #endregion Constants

        #region Static variables

        private static string? _userAgent;

        #endregion Static variables

        #region Internal methods

        /// <summary>
        ///     Checks a password against the Have I Been Pwned API
        /// </summary>
        /// <param name="password">Password that should be checked</param>
        /// <param name="isInputHashed">True if the input is already hashed</param>
        /// <returns>Number of breaches the passed password was part of</returns>
        internal static async Task<int> CheckPassword(string password, bool isInputHashed = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

            var hash = isInputHashed ? password : Sha1Helper.Hash(password);

            var firstPart = hash[..5];
            var lastPart = hash[5..];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(GetUserAgent());

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUri}/{firstPart}");
            using var response = await client.SendAsync(request);

            // Status 404 means, the password is not breached yet
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return 0;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(result))
            {
                return 0;
            }

            var lines = result.Split([ "\r\n", "\r", "\n" ], StringSplitOptions.None);
            var pwdLine = lines.FirstOrDefault(l => l.StartsWith(lastPart));

            if (!string.IsNullOrWhiteSpace(pwdLine))
            {
                var countStr = pwdLine.Split(':').Last();

                if (int.TryParse(countStr, out var numberOfBreaches))
                {
                    return numberOfBreaches;
                }

                throw new InvalidOperationException($"No valid number returned (got {countStr})!");
            }

            return 0;
        }

        #endregion Internal methods

        #region Private methods

        private static string GetUserAgent()
        {
            if (_userAgent != null)
            {
                return _userAgent;
            }

            return _userAgent = $"PasswordChecker/{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        #endregion Private methods
    }
}
