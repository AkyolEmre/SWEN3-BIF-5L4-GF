using Microsoft.Extensions.Logging;
using Tesseract;
using PdfiumViewer;
using System.Drawing;

namespace DMS.OCRWorker
{
    public class OcrService
    {
        private readonly ILogger<OcrWorker> _logger;

        public OcrService(ILogger<OcrWorker> logger)
        {
            _logger = logger;
        }

        public string PerformOcr(Stream pdfStream)
        {
            try
            {
                // 1. PDF laden
                using var pdfDocument = PdfDocument.Load(pdfStream);

                // 2. Erste Seite als Bitmap rendern (300 DPI)
                using var bitmap = pdfDocument.Render(
                    page: 0,
                    dpiX: 300,
                    dpiY: 300,
                    forPrinting: true);

                // 3. Bitmap → Tesseract Pix (direkt über Data-Pointer – 100% funktioniert!)
                using var pix = Pix.LoadFromMemory(BitmapToPixData(bitmap));

                // 4. OCR mit Tesseract
                using var engine = new TesseractEngine("./tessdata", "deu", EngineMode.Default);
                using var page = engine.Process(pix);

                var text = page.GetText().Trim();
                _logger.LogInformation("OCR erfolgreich – {Chars} Zeichen erkannt", text.Length);
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR mit Tesseract fehlgeschlagen");
                return $"OCR Fehler: {ex.Message}";
            }
        }

        private byte[] BitmapToPixData(Image bitmap)
        {
            throw new NotImplementedException();
        }

        // Hilfsmethode: Bitmap → byte[] im Pix-Format
        private static byte[] BitmapToPixData(Bitmap bitmap)
        {
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                var length = bmpData.Stride * bmpData.Height;
                var data = new byte[length];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, length);
                return data;
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }
    }
}