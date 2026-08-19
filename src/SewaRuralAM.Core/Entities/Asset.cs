using SewaRuralAM.Core.Enums;

namespace SewaRuralAM.Core.Entities;

public class AssetCategory : BaseEntity
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<AssetPropertyDefinition> PropertyDefinitions { get; set; } = new List<AssetPropertyDefinition>();
}

public class Asset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;

    public int AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; } = null!;

    public string? AssetType { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? Vendor { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public string? Description { get; set; }
    public string? QrCodeData { get; set; }
    public string? PhotoPath { get; set; }

    public bool IsVerified { get; set; }
    public DateTime? LastVerifiedDate { get; set; }

    public ICollection<AssetPropertyValue> PropertyValues { get; set; } = new List<AssetPropertyValue>();
    public ICollection<AssetLocationMapping> LocationMappings { get; set; } = new List<AssetLocationMapping>();
    public ICollection<VerificationLog> VerificationLogs { get; set; } = new List<VerificationLog>();
    public ICollection<QrPrintLog> QrPrintLogs { get; set; } = new List<QrPrintLog>();
}

public class AssetPropertyDefinition : BaseEntity
{
    public int AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; } = null!;

    public string PropertyName { get; set; } = string.Empty;
    public PropertyDataType DataType { get; set; } = PropertyDataType.Text;
    public string? DropdownOptions { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<AssetPropertyValue> PropertyValues { get; set; } = new List<AssetPropertyValue>();
}

public class AssetPropertyValue : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int AssetPropertyDefinitionId { get; set; }
    public AssetPropertyDefinition AssetPropertyDefinition { get; set; } = null!;

    public string? Value { get; set; }
}

public class AssetLocationMapping : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedDate { get; set; }
    public string? Remarks { get; set; }

    public bool IsCurrent { get; set; } = true;
}

public class VerificationLog : BaseEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int VerifiedByUserId { get; set; }
    public User VerifiedByUser { get; set; } = null!;

    public DateTime VerifiedDate { get; set; } = DateTime.UtcNow;
    public double? GpsLatitude { get; set; }
    public double? GpsLongitude { get; set; }
    public string? Remarks { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
}

public class LocationVerificationLog : BaseEntity
{
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int VerifiedByUserId { get; set; }
    public User VerifiedByUser { get; set; } = null!;

    public DateTime VerifiedDate { get; set; } = DateTime.UtcNow;
    public double? GpsLatitude { get; set; }
    public double? GpsLongitude { get; set; }
    public string? Remarks { get; set; }
}

public class QrPrintLog : BaseEntity
{
    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int PrintedByUserId { get; set; }
    public User PrintedByUser { get; set; } = null!;

    public DateTime PrintedDate { get; set; } = DateTime.UtcNow;
    public int Quantity { get; set; } = 1;
}
