using System.Windows;

namespace PasswordChecker.UI.Windows
{
    /// <summary>
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow
    {
        public AuthenticationWindow()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }
    }
}
