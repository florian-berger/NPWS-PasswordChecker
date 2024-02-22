using PasswordChecker.Data;
using PasswordChecker.Pdf.Helper;
using PasswordChecker.Resources.Language;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Data;
using System.Drawing;
using System.Globalization;
using PasswordChecker.Resources;
using PasswordChecker.Shared.Extensions;

namespace PasswordChecker.Pdf
{
    public class Generator
    {
        #region Private variables

        private readonly ReportData _reportData;
        private readonly LogonData? _logonData;

        private readonly CultureInfo _usedCulture;

        #endregion Private variables

        #region Constructor

        public Generator(ReportData reportData, LogonData? logonData)
        {
            ArgumentNullException.ThrowIfNull(reportData);

            _reportData = reportData;
            _logonData = logonData;

            // Hack: Why is the new Task not using the correct stuff?
            _usedCulture = LanguageHelper.GetCurrentCulture();
            CultureInfo.CurrentCulture = _usedCulture;
            CultureInfo.CurrentUICulture = _usedCulture;
        }

        #endregion Constructor

        #region Public methods

        public Task Generate(string fileName)
        {
            return Task.Run(() =>
            {
                var document = new PdfDocument();

                CreateSummaryPage(document);

                if (_reportData.BreachedPasswords.Count > 0)
                {
                    CreateBreachedPasswordsPage(document);
                }

                if (_reportData.Duplicates.Length > 0)
                {
                    CreateDuplicatesPage(document);
                }

                if (_reportData.Errors.Count > 0)
                {
                    CreateErrorsPage(document);
                }

                document.Save(fileName);
                document.Close(true);
            });
        }

        #endregion Public methods

        #region Private methods

        private void CreateSummaryPage(PdfDocument document)
        {
            var page = PageHelper.CreatePage(document, PdfFileResource.SummaryPageTitle, false);
            var graphics = page.Graphics;

            var titleFont = GraphicsHelper.GetTitleFont();
            var subtitleFont = GraphicsHelper.GetTitleFont();

            var titleElement = new PdfTextElement(PdfFileResource.Heading, titleFont,
                new PdfSolidBrush(new PdfColor(70, 130, 180)));
            var subtitleElement = new PdfTextElement("by Florian Berger & Contributors", subtitleFont,
                new PdfSolidBrush(new PdfColor(70, 130, 180)));

            var layoutFormat = new PdfLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
                Break = PdfLayoutBreakType.FitPage
            };

            var layoutGridFormat = new PdfGridLayoutFormat
            {
                Break = PdfLayoutBreakType.FitPage
            };

            var titleSize = titleFont.MeasureString(titleElement.Text);
            var subtitleSize = subtitleFont.MeasureString(subtitleElement.Text);

            var titleX = graphics.ClientSize.Width / 2 - titleSize.Width / 2;
            var result = titleElement.Draw(page, new PointF(titleX, 0), layoutFormat);

            var subtitleX = graphics.ClientSize.Width / 2 - subtitleSize.Width / 2;
            result = subtitleElement.Draw(page, new PointF(subtitleX, result.LastLineBounds.Y + 20), layoutFormat);

            var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);

            var textElement = new PdfTextElement(PdfFileResource.CheckGeneralInfo, headerFont, PdfBrushes.Black);
            result = textElement.Draw(page, new PointF(0, result.LastLineBounds.Y + 40), layoutFormat);

            var generalTable = new DataTable();
            generalTable.Columns.Add(" ");
            generalTable.Columns.Add("  ");
            generalTable.Rows.Add(PdfFileResource.CheckStart, _reportData.CheckStart);
            generalTable.Rows.Add(PdfFileResource.CheckEnd, _reportData.CheckEnd);
            generalTable.Rows.Add(PdfFileResource.CheckDuration,
                _reportData.Duration.ToReadableFullText(PdfFileResource.DurationDays, PdfFileResource.DurationHours,
                    PdfFileResource.DurationMinutes, PdfFileResource.DurationSeconds));

            if (_logonData != null)
            {
                generalTable.Rows.Add(PdfFileResource.ConnectionInfo, " ");
                generalTable.Rows.Add($"    {PdfFileResource.CheckServerAddress}", _logonData.ServerAddress);
                generalTable.Rows.Add($"    {PdfFileResource.CheckDatabaseName}", _logonData.DatabaseName);
                generalTable.Rows.Add($"    {PdfFileResource.CheckUserName}", _logonData.UserDisplayName ?? _logonData.UserName);
            }

            var generalGrid = new PdfGrid
            {
                DataSource = generalTable
            };
            generalGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.ListTable1LightAccent1);
            var gridResult = generalGrid.Draw(result.Page,
                new RectangleF(0, result.LastLineBounds.Y + 5, page.GetClientSize().Width, 300), layoutGridFormat);

            var textElement2 = new PdfTextElement(PdfFileResource.CheckFieldsOverview, headerFont, PdfBrushes.Black);
            result = textElement2.Draw(gridResult.Page, new PointF(0, gridResult.Bounds.Bottom + 15), layoutFormat);

            var infoTable = new DataTable();
            infoTable.Columns.Add(" ");
            infoTable.Columns.Add("  ");
            infoTable.Rows.Add(PdfFileResource.CheckTotalFieldsCount, _reportData.TotalCountOfPasswordFields.ToString("N0", _usedCulture));
            infoTable.Rows.Add(PdfFileResource.CheckSkippedFieldsCount, _reportData.SkippedCountOfPasswordFields.ToString("N0", _usedCulture));
            infoTable.Rows.Add(PdfFileResource.CheckDuplicates, _reportData.DuplicatesCount.ToString("N0", _usedCulture));
            infoTable.Rows.Add(PdfFileResource.CheckBreached, _reportData.BreachedPasswords.Count.ToString("N0", _usedCulture));
            infoTable.Rows.Add(PdfFileResource.CheckErrors, _reportData.Errors.Count.ToString("N0", _usedCulture));
            infoTable.Rows.Add(PdfFileResource.CheckQuality, "");
            infoTable.Rows.Add($"     {PdfFileResource.QualityWeak}", _reportData.Quality.WeakPasswords.Count);
            infoTable.Rows.Add($"     {PdfFileResource.QualityGood}", _reportData.Quality.GoodPasswords.Count);
            infoTable.Rows.Add($"     {PdfFileResource.QualityStrong}", _reportData.Quality.StrongPasswords.Count);

            var infoGrid = new PdfGrid
            {
                DataSource = infoTable
            };
            infoGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.ListTable1LightAccent1);
            infoGrid.Draw(result.Page, new RectangleF(0, result.LastLineBounds.Y + 5, page.GetClientSize().Width, page.GetClientSize().Height - 35),
                layoutGridFormat);
        }

        private void CreateBreachedPasswordsPage(PdfDocument document)
        {
            var page = PageHelper.CreatePage(document, PdfFileResource.BreachedPageTitle);
            var breachedDataTable = new DataTable();

            breachedDataTable.Columns.Add(PdfFileResource.BreachedPasswordIdentifier);
            breachedDataTable.Columns.Add(PdfFileResource.BreachedCount);

            foreach (var breached in _reportData.BreachedPasswords.OrderByDescending(b => b.Count))
            {
                breachedDataTable.Rows.Add(breached.Identifier, breached.Count.ToString("N0", _usedCulture));
            }

            var breachedGrid = new PdfGrid
            {
                DataSource = breachedDataTable
            };
            breachedGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.ListTable1LightAccent1);

            var layoutFormat = new PdfGridLayoutFormat
            {
                Break = PdfLayoutBreakType.FitPage
            };
            breachedGrid.Draw(page, new RectangleF(0, 35, page.GetClientSize().Width, page.GetClientSize().Height - 35),
                layoutFormat);
        }

        private void CreateDuplicatesPage(PdfDocument document)
        {
            var page = PageHelper.CreatePage(document, PdfFileResource.DuplicatesPageTitle);
            var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);
            var passNamesFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10f);

            var layoutFormat = new PdfLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
                Break = PdfLayoutBreakType.FitPage
            };

            PdfTextLayoutResult? lastResult = null;

            // List all duplicates
            foreach (var duplicates in _reportData.Duplicates)
            {
                var text = string.Format(PdfFileResource.DuplicatesHeading, duplicates.Count);
                var textElement = new PdfTextElement(text, headerFont, PdfBrushes.Black);

                var y = lastResult != null ? lastResult.LastLineBounds.Y + 30 : 35;
                lastResult = textElement.Draw(page,
                    new RectangleF(0, y, page.GetClientSize().Width, page.GetClientSize().Height),
                    layoutFormat);
                page = lastResult.Page;

                var c = 0;
                foreach (var passIdentifier in duplicates)
                {
                    var newElement = new PdfTextElement(passIdentifier, passNamesFont, PdfBrushes.Black);
                    lastResult = newElement.Draw(page,
                        new RectangleF(0, lastResult.LastLineBounds.Location.Y + (c == 0 ? 18 : 14),
                            page.GetClientSize().Width,
                            page.GetClientSize().Height), layoutFormat);
                    page = lastResult.Page;

                    c++;
                }
            }
        }

        private void CreateErrorsPage(PdfDocument document)
        {
            var page = PageHelper.CreatePage(document, PdfFileResource.ErrorsPageTitle);
            var passNameFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);
            var exceptionFont = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);

            var layoutFormat = new PdfLayoutFormat
            {
                Layout = PdfLayoutType.Paginate,
                Break = PdfLayoutBreakType.FitPage
            };

            PdfTextLayoutResult? lastResult = null;
            foreach (var error in _reportData.Errors)
            {
                // First, add the identifier of the password
                var textElement = new PdfTextElement(error.Key, passNameFont, PdfBrushes.Black);
                var y = lastResult != null ? lastResult.LastLineBounds.Y + 30 : 35;

                lastResult = textElement.Draw(page,
                    new RectangleF(0, y, page.GetClientSize().Width, page.GetClientSize().Height), layoutFormat);
                page = lastResult.Page;

                // Then add the exception details
                var textElement2 = new PdfTextElement(error.Value.ToString(), exceptionFont);
                var y2 = lastResult.LastLineBounds.Y + 18;

                lastResult = textElement2.Draw(page,
                    new RectangleF(0, y2, page.GetClientSize().Width, page.GetClientSize().Height), layoutFormat);
                page = lastResult.Page;
            }
        }

        #endregion Private methods
    }
}
