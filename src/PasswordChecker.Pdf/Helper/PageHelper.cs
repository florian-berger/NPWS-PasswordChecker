using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using System.Drawing;

namespace PasswordChecker.Pdf.Helper
{
    /// <summary>
    ///     Helper for PDF pages
    /// </summary>
    internal class PageHelper
    {
        #region Internal methods

        /// <summary>
        ///     Creates a new PDF page and returns it
        /// </summary>
        /// <param name="document">Document the page should be created in</param>
        /// <param name="title">Title of the page</param>
        /// <param name="createHeader">True if a header should be generated</param>
        /// <param name="toUpper">True if the header should be printed uppercase</param>
        internal static PdfPage CreatePage(PdfDocument document, string title, bool createHeader = true, bool toUpper = true)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            // Create a new page
            var page = document.Pages.Add();

            // Add destinations
            var destination = new PdfNamedDestination(title)
            {
                Destination = new PdfDestination(page)
            };
            document.NamedDestinationCollection.Add(destination);

            // Add a bookmark to allow in-file navigation
            var bookmark = document.Bookmarks.Add(title);
            bookmark.NamedDestination = destination;

            // Draw title, if required
            if (createHeader && !string.IsNullOrWhiteSpace(title))
            {
                DrawPageTitle(page, title, toUpper);
            }

            return page;
        }

        #endregion Internal methods

        #region Private methods

        private static void DrawPageTitle(PdfPage page, string title, bool toUpper)
        {
            var graphics = page.Graphics;
            var font = new PdfStandardFont(PdfFontFamily.TimesRoman, 14f, PdfFontStyle.Bold);

            var backgroundBrush = new PdfSolidBrush(new PdfColor(70, 130, 180));
            var bounds = new RectangleF(0, 0, graphics.ClientSize.Width, 30);

            graphics.DrawRectangle(backgroundBrush, bounds);

            title = toUpper ? title.ToUpper() : title;
            var element = new PdfTextElement(title, font)
            {
                Brush = PdfBrushes.White
            };

            var headerSize = font.MeasureString(element.Text);
            var x = graphics.ClientSize.Width / 2 - headerSize.Width / 2;
            element.Draw(page, new PointF(x, bounds.Top + 8));
        }

        #endregion Private methods
    }
}
