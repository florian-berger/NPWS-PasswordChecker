using System.Diagnostics;
using System.Runtime.InteropServices;
using Syncfusion.Windows.Shared;

namespace PasswordChecker.UI.Wpf
{
    /// <summary>
    ///     Static commands that can be used in the whole software
    /// </summary>
    internal class StaticCommands
    {
        #region Commands

        /// <summary>
        ///     Command for opening a URI
        /// </summary>
        public static DelegateCommand<string> OpenUriCommand => _openUriCommand ??= new DelegateCommand<string>(OpenUri);
        private static DelegateCommand<string>? _openUriCommand;

        #endregion Commands

        #region Private methods

        private static void OpenUri(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(address) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", address);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", address);
            }
        }

        #endregion Private methods
    }
}
