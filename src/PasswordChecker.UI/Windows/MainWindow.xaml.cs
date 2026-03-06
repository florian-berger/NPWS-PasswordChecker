using PasswordChecker.UI.ViewModel;
using System;
using System.Windows.Controls;

namespace PasswordChecker.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowRibbon_OnRibbonContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void MainWindow_OnContentRendered(object? sender, EventArgs e)
        {
            (DataContext as MainViewModel)?.CheckForVersionUpdate();
        }
    }
}
