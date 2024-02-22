using Syncfusion.Pdf.Graphics;

namespace PasswordChecker.Pdf.Helper
{
    internal static class GraphicsHelper
    {
        internal static PdfStandardFont GetTitleFont()
        {
            return new PdfStandardFont(PdfFontFamily.Helvetica, 20f, PdfFontStyle.Bold);
        }

        internal static PdfStandardFont GetSubtitleFont()
        {
            var titleFont = GetTitleFont();
            return new PdfStandardFont(titleFont, 16f);
        }
    }
}
