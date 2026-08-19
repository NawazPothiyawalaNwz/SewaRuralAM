using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.Infrastructure.Services;

public class PdfService : IPdfService
{
    private static readonly XColor HeaderColor = XColor.FromArgb(0xFF, 0x6B, 0x1B, 0x2E);
    private static readonly XColor BorderColor = XColor.FromArgb(0xFF, 0xE5, 0xDA, 0xD1);

    static PdfService()
    {
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
    }

    public byte[] GenerateAssetQrSheet(IEnumerable<(string AssetCode, string AssetName, byte[] QrImage)> assets)
    {
        var items = assets.ToList();

        const double margin = 30;
        const int columns = 3;
        const double cellWidth = (595 - 2 * margin) / columns;
        const double cellHeight = 190;
        const double qrSize = 130;

        var document = new PdfDocument();
        var codeFont = new XFont("Poppins", 10, XFontStyle.Bold);
        var nameFont = new XFont("Poppins", 8, XFontStyle.Regular);

        PdfPage? page = null;
        XGraphics? gfx = null;
        var rowsPerPage = (int)((842 - 2 * margin) / cellHeight);
        var perPage = columns * rowsPerPage;

        for (var i = 0; i < items.Count; i++)
        {
            var indexOnPage = i % perPage;
            if (indexOnPage == 0)
            {
                page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
            }

            var col = indexOnPage % columns;
            var row = indexOnPage / columns;
            var x = margin + col * cellWidth;
            var y = margin + row * cellHeight;

            var item = items[i];
            gfx!.DrawRectangle(new XPen(BorderColor), x, y, cellWidth - 8, cellHeight - 8);

            using var qrStream = new MemoryStream(item.QrImage);
            var qrImage = XImage.FromStream(() => qrStream);
            var qrX = x + (cellWidth - 8 - qrSize) / 2;
            gfx.DrawImage(qrImage, qrX, y + 8, qrSize, qrSize);

            gfx.DrawString(item.AssetCode, codeFont, XBrushes.Black,
                new XRect(x, y + qrSize + 14, cellWidth - 8, 16), XStringFormats.TopCenter);
            gfx.DrawString(item.AssetName, nameFont, XBrushes.Gray,
                new XRect(x + 4, y + qrSize + 32, cellWidth - 16, 30), XStringFormats.TopCenter);
        }

        return SaveToBytes(document);
    }

    public byte[] GenerateAssetRegisterReport(IEnumerable<Asset> assets)
    {
        var columns = new (string Title, double Width)[]
        {
            ("Asset Code", 90),
            ("Asset Name", 190),
            ("Category", 110),
            ("Brand", 100),
            ("Status", 90),
            ("Purchase Date", 100)
        };

        var rows = assets.Select(asset => new[]
        {
            asset.AssetCode,
            asset.AssetName,
            asset.AssetCategory?.CategoryName ?? "-",
            asset.Brand ?? "-",
            asset.Status.ToString(),
            asset.PurchaseDate?.ToString("yyyy-MM-dd") ?? "-"
        });

        return DrawTableReport("Asset Register", columns, rows);
    }

    public byte[] GenerateAssetVerificationReport(IEnumerable<VerificationLog> logs)
    {
        var columns = new (string Title, double Width)[]
        {
            ("Asset Code", 90),
            ("Asset Name", 190),
            ("Verified Date", 110),
            ("Verified By", 140),
            ("Location", 140),
            ("Remarks", 130)
        };

        var rows = logs
            .OrderByDescending(l => l.VerifiedDate)
            .Select(log => new[]
            {
                log.Asset?.AssetCode ?? "-",
                log.Asset?.AssetName ?? "-",
                log.VerifiedDate.ToString("yyyy-MM-dd HH:mm"),
                log.VerifiedByUser?.FullName ?? "-",
                log.Location?.LocationName ?? "-",
                log.Remarks ?? "-"
            });

        return DrawTableReport("Asset Verification Report", columns, rows);
    }

    public byte[] GenerateLocationVerificationReport(IEnumerable<LocationVerificationLog> logs)
    {
        var columns = new (string Title, double Width)[]
        {
            ("Location Code", 110),
            ("Location Name", 220),
            ("Verified Date", 130),
            ("Verified By", 160),
            ("Remarks", 160)
        };

        var rows = logs
            .OrderByDescending(l => l.VerifiedDate)
            .Select(log => new[]
            {
                log.Location?.LocationCode ?? "-",
                log.Location?.LocationName ?? "-",
                log.VerifiedDate.ToString("yyyy-MM-dd HH:mm"),
                log.VerifiedByUser?.FullName ?? "-",
                log.Remarks ?? "-"
            });

        return DrawTableReport("Location Verification Report", columns, rows);
    }

    private byte[] DrawTableReport(string title, (string Title, double Width)[] columns, IEnumerable<string[]> rows)
    {
        const double margin = 30;
        const double rowHeight = 22;
        const double pageWidth = 842;
        const double pageHeight = 595;

        var document = new PdfDocument();
        var titleFont = new XFont("Poppins", 16, XFontStyle.Bold);
        var headerFont = new XFont("Poppins", 10, XFontStyle.Bold);
        var cellFont = new XFont("Poppins", 9, XFontStyle.Regular);

        XGraphics? gfx = null;
        double y = 0;

        void StartPage()
        {
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(pageWidth);
            page.Height = XUnit.FromPoint(pageHeight);
            gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString(title, titleFont, new XSolidBrush(HeaderColor), new XRect(margin, margin, pageWidth - 2 * margin, 24), XStringFormats.TopLeft);
            y = margin + 34;

            DrawHeaderRow();
        }

        void DrawHeaderRow()
        {
            var x = margin;
            gfx!.DrawRectangle(new XSolidBrush(HeaderColor), margin, y, pageWidth - 2 * margin, rowHeight);
            foreach (var (colTitle, width) in columns)
            {
                gfx.DrawString(colTitle, headerFont, XBrushes.White, new XRect(x + 4, y, width - 4, rowHeight), XStringFormats.CenterLeft);
                x += width;
            }
            y += rowHeight;
        }

        StartPage();

        var hasRows = false;
        foreach (var values in rows)
        {
            hasRows = true;
            if (y + rowHeight > pageHeight - margin)
                StartPage();

            var x = margin;
            gfx!.DrawRectangle(new XPen(BorderColor), margin, y, pageWidth - 2 * margin, rowHeight);
            for (var i = 0; i < columns.Length && i < values.Length; i++)
            {
                gfx.DrawString(values[i], cellFont, XBrushes.Black, new XRect(x + 4, y, columns[i].Width - 4, rowHeight), XStringFormats.CenterLeft);
                x += columns[i].Width;
            }
            y += rowHeight;
        }

        if (!hasRows)
        {
            gfx!.DrawString("No records to display.", cellFont, XBrushes.Gray, new XRect(margin, y + 8, pageWidth - 2 * margin, rowHeight), XStringFormats.TopLeft);
        }

        return SaveToBytes(document);
    }

    private static byte[] SaveToBytes(PdfDocument document)
    {
        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
