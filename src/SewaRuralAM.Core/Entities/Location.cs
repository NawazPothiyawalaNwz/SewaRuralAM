namespace SewaRuralAM.Core.Entities;

public class Location : BaseEntity
{
    public const int MaxLevel = 6;

    public int? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }

    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public int LevelNo { get; set; } = 1;
    public string? QrCodeData { get; set; }

    public bool IsVerified { get; set; }
    public DateTime? LastVerifiedDate { get; set; }

    public ICollection<Location> ChildLocations { get; set; } = new List<Location>();
    public ICollection<AssetLocationMapping> AssetLocationMappings { get; set; } = new List<AssetLocationMapping>();
    public ICollection<LocationVerificationLog> VerificationLogs { get; set; } = new List<LocationVerificationLog>();
}
