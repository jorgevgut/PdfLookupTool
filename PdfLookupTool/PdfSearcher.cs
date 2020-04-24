using iText.Layout.Element;
using iText;
using iText.Kernel.Pdf;
using System.IO;
using System;
using System.Windows.Controls;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Globalization;
using System.Text;

namespace PdfLookupTool
{
    class PdfSearcher
    {
        private PdfReader Pdf = null;
        private PdfDocument PdfDocument = null;
        private ITextExtractionStrategy textExtractionStrategy;
        public PdfSearcher(string filename)
        {
            if (File.Exists(filename))
            {
                Pdf = new PdfReader(filename);
                PdfDocument = new PdfDocument(Pdf);
                textExtractionStrategy = new SimpleTextExtractionStrategy();
            } else
            {
                throw new FileNotFoundException($"Did not find {filename}");
            }
        }

        // Found this on stack overflow, credits -> https://stackoverflow.com/questions/359827/ignoring-accented-letters-in-string-comparison 
        private string RemoveDiacritics(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public int Ocurrences(string searchTerm)
        {
            var count = 0;
            var totalPages = PdfDocument.GetNumberOfPages();
            for (int page = 1; page <= totalPages; page++)
            {
                FilteredTextEventListener listener = new FilteredTextEventListener(textExtractionStrategy);
                var currentPage = PdfDocument.GetPage(page);
                new PdfCanvasProcessor(listener).ProcessPageContent(currentPage);
                if (RemoveDiacritics(textExtractionStrategy.GetResultantText().ToLowerInvariant())
                    .Contains(RemoveDiacritics(searchTerm.ToLowerInvariant())))
                {
                    count++;
                }
            }
            return count;
        }

        public void Close()
        {
            Pdf.Close();
        }
    }
}