using System;
using System.Windows;
using System.Windows.Controls;
using PasswordChecker.Data;
using PasswordChecker.UI.ViewModel;
using Syncfusion.UI.Xaml.Charts;

namespace PasswordChecker.UI.Windows
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow
    {
        public ReportWindow(ReportData reportData, LogonData? logonData)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();

            DataContext = new ReportViewModel(reportData, logonData);
        }

        private void Chart_OnInitialized(object? sender, EventArgs e)
        {
            if (sender is not SfChart chart)
            {
                return;
            }

            chart.Legend = new ChartLegend
            {
                DockPosition = ChartDock.Bottom
            };
        }

        private void Series_OnInitialized(object? sender, EventArgs e)
        {
            if (sender is not PieSeries series)
            {
                return;
            }

            series.AdornmentsInfo = new ChartAdornmentInfo
            {
                ShowLabel = true,
                UseSeriesPalette = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void OnRibbonContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}
