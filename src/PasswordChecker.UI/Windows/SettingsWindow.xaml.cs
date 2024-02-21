using System.Windows.Controls;

namespace PasswordChecker.UI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void WindowRibbon_OnRibbonContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}
