using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Enums;
using SewaRuralAM.Infrastructure.Services;
using Xunit;

namespace SewaRuralAM.Tests;

public class PdfServiceTests
{
    [Fact]
    public void GenerateAssetRegisterReport_ProducesNonEmptyPdf()
    {
        var service = new PdfService();
        var category = new AssetCategory { Id = 1, CategoryName = "Computers" };
        var assets = new List<Asset>
        {
            new()
            {
                AssetCode = "AST-0001",
                AssetName = "Test Laptop",
                AssetCategory = category,
                Brand = "Dell",
                Status = AssetStatus.Active,
                PurchaseDate = DateTime.UtcNow
            }
        };

        var bytes = service.GenerateAssetRegisterReport(assets);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void GenerateAssetQrSheet_ProducesNonEmptyPdf()
    {
        var service = new PdfService();
        var qrService = new QrCodeService();
        var qrBytes = qrService.GenerateQrCode("ASSET|1|AST-0001");

        var items = new List<(string, string, byte[])>
        {
            ("AST-0001", "Test Laptop", qrBytes)
        };

        var bytes = service.GenerateAssetQrSheet(items);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void GenerateAssetVerificationReport_ProducesNonEmptyPdf()
    {
        var service = new PdfService();
        var user = new User { UserName = "admin", FullName = "System Administrator" };
        var asset = new Asset { AssetCode = "AST-0001", AssetName = "Test Laptop" };
        var location = new Location { LocationCode = "LOC-A", LocationName = "Shelf A" };

        var logs = new List<VerificationLog>
        {
            new() { Asset = asset, VerifiedByUser = user, Location = location, VerifiedDate = DateTime.UtcNow, Remarks = "OK" }
        };

        var bytes = service.GenerateAssetVerificationReport(logs);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void GenerateAssetVerificationReport_WithNoLogs_StillProducesValidPdf()
    {
        var service = new PdfService();

        var bytes = service.GenerateAssetVerificationReport(Enumerable.Empty<VerificationLog>());

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void GenerateLocationVerificationReport_ProducesNonEmptyPdf()
    {
        var service = new PdfService();
        var user = new User { UserName = "admin", FullName = "System Administrator" };
        var location = new Location { LocationCode = "LOC-A", LocationName = "Shelf A" };

        var logs = new List<LocationVerificationLog>
        {
            new() { Location = location, VerifiedByUser = user, VerifiedDate = DateTime.UtcNow }
        };

        var bytes = service.GenerateLocationVerificationReport(logs);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
