using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using PasswordChecker.Data;
using PasswordChecker.Pdf;
using PasswordChecker.Resources.Language;
using PasswordChecker.Shared.Helpers;
using PasswordChecker.UI.Enums;
using PasswordChecker.UI.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

namespace PasswordChecker.UI.ViewModel
{
    internal class ReportViewModel(ReportData data, LogonData? logonData, Window windowInstance) : BindableBase
    {
        #region Properties

        public ReportData Report { get; } = data;

        public List<Tuple<string, int>> ReportQualityChartData => _chartData ??= GenerateChartData();
        private List<Tuple<string, int>>? _chartData;

        #endregion Properties

        #region Commands

        /// <summary>
        ///     Command to save the report as PDf file
        /// </summary>
        public DelegateCommand SavePdfCommand => _savePdfCommand ??= new DelegateCommand(SavePdf);
        private DelegateCommand? _savePdfCommand;

        #endregion Commands

        #region Private methods

        private void SavePdf()
        {
            var fileDlg = new SaveFileDialog
            {
                Filter = "Portable Document Format (*.pdf)|*.pdf",
                FileName = "",
                Title = ReportResource.SelectExportFile,
                ValidateNames = true,
                OverwritePrompt = true,
                AddExtension = true,
                AddToRecent = true
            };

            if (fileDlg.ShowDialog(windowInstance) == true)
            {
                _ = SaveFile(fileDlg.FileName);
            }
        }

        private async Task SaveFile(string targetFile)
        {
            if (File.Exists(targetFile))
            {
                // First, try to move to recycle bin via FileSystem. If it fails, delete via framework functionality
                try
                {
                    FileSystem.DeleteFile(targetFile, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch
                {
                    File.Delete(targetFile);
                }
            }

            try
            {
                await new Generator(Report, logonData).Generate(targetFile);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowErrorDialog(ex, windowInstance);
                return;
            }

            var result = CustomMessageBox.ShowDialog(ReportResource.ReportExportSucceeded,
                ReportResource.SuccessfullySaved, CustomMessageBoxButtons.YesNo, CustomMessageBoxImage.Question,
                windowInstance);
            if (result == CustomMessageBoxResult.Yes)
            {
                ProcessHelper.OpenFileInDefaultProgram(targetFile);
            }
        }

        private List<Tuple<string, int>> GenerateChartData()
        {
            return
            [
                new Tuple<string, int>(ReportResource.QualityWeak, Report.Quality.WeakPasswords.Count),
                new Tuple<string, int>(ReportResource.QualityGood, Report.Quality.GoodPasswords.Count),
                new Tuple<string, int>(ReportResource.QualityStrong, Report.Quality.StrongPasswords.Count)
            ];
        }

        #endregion Private methods
    }
}
