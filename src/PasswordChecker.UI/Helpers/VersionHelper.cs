using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordChecker.UI.Helpers
{
    internal static class VersionHelper
    {
        public static async Task<(Version Version, string ReleaseLink)?> CheckForUpdate(Version currentVersion)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"NpwsPasswordChecker/{GetVersionString(currentVersion)}");

            // Read the latest release information from GitHub and parse the JSON
            var response = await client.GetStringAsync("https://api.github.com/repos/florian-berger/NPWS-PasswordChecker/releases/latest");
            var json = JsonDocument.Parse(response);

            // Search the tag name and trim the leading 'v' (tag will always be named in the format vX.Y.Z)
            var versionString = json.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
            var newestVersion = Version.Parse(versionString.TrimStart('v'));
            var releaseLink = json.RootElement.GetProperty("html_url").GetString() ?? string.Empty;

            return newestVersion > currentVersion ? new ValueTuple<Version, string>(newestVersion, releaseLink) : null;
        }

        public static string GetVersionString(Version version)
        {
            var versionStr = $"{version.Major}.{version.Minor}";
            if (version.Build > 0)
            {
                versionStr += $".{version.Build}";
            }

            return versionStr;
        }
    }
}
