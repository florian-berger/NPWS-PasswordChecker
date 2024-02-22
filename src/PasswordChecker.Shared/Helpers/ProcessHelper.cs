using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PasswordChecker.Shared.Helpers
{
    /// <summary>
    ///     Static class providing helper methods for processes
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        ///     Opens the passed file in the default program
        /// </summary>
        /// <param name="fileName">File that should be opened</param>
        public static void OpenFileInDefaultProgram(string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", fileName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", fileName);
            }
        }
    }
}
