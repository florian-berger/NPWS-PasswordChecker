using System.Windows;

namespace PasswordChecker.UI.Windows
{
    /// <summary>
    /// Interaction logic for ThirdPartyLicensesWindow.xaml
    /// </summary>
    public partial class ThirdPartyLicensesWindow
    {
        public ThirdPartyLicensesWindow(Window? owner = null)
        {
            Owner = owner ?? Application.Current.MainWindow;

            InitializeComponent();
        }
    }
}
