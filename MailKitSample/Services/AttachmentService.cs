using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharp.Drawing;
    
namespace MailKitSample.Services
{
    public class AttachmentService : IAttachmentService
    {
        public string GenerateRandomAttachment()
        {
            var rand = new Random();
            int type = rand.Next(3);
            string file;

            switch (type)
            {
                case 0:
                    file = Path.GetTempFileName().Replace(".tmp", ".xlsx");
                    CreateExcel(file);
                    break;
                case 1:
                    file = Path.GetTempFileName().Replace(".tmp", ".docx");
                    CreateWord(file);
                    break;
                default:
                    file = Path.GetTempFileName().Replace(".tmp", ".pdf");
                    CreatePdf(file);
                    break;
            }

            return file;
        }

        private void CreateExcel(string file)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("データ");
            ws.Cell(1, 1).Value = "テストデータ";
            ws.Cell(2, 1).Value = new Random().Next(1, 1000);
            wb.SaveAs(file);
        }

        private void CreateWord(string file)
        {
            using var doc = WordprocessingDocument.Create(file, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body(new Paragraph(new Run(new DocumentFormat.OpenXml.Drawing.Text("自動生成された Word ファイル")))));
            mainPart.Document.Save();
        }

        private void CreatePdf(string file)
        {
            using var pdf = new PdfSharp.Pdf.PdfDocument();
            var page = pdf.AddPage();
            var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharp.Drawing.XFont("Arial", 14);
            gfx.DrawString("自動生成された PDF ファイル", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            pdf.Save(file);
        }
    }
}
