using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Infrastructure.Data.Configurations;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasIndex(c => c.CategoryName).IsUnique();
        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(150);
    }
}

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasIndex(a => a.AssetCode).IsUnique();
        builder.Property(a => a.AssetCode).IsRequired().HasMaxLength(50);
        builder.Property(a => a.AssetName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.PurchaseCost).HasPrecision(18, 2);

        builder.HasOne(a => a.AssetCategory)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.AssetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AssetPropertyDefinitionConfiguration : IEntityTypeConfiguration<AssetPropertyDefinition>
{
    public void Configure(EntityTypeBuilder<AssetPropertyDefinition> builder)
    {
        builder.Property(p => p.PropertyName).IsRequired().HasMaxLength(150);

        builder.HasOne(p => p.AssetCategory)
            .WithMany(c => c.PropertyDefinitions)
            .HasForeignKey(p => p.AssetCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssetPropertyValueConfiguration : IEntityTypeConfiguration<AssetPropertyValue>
{
    public void Configure(EntityTypeBuilder<AssetPropertyValue> builder)
    {
        builder.HasOne(v => v.Asset)
            .WithMany(a => a.PropertyValues)
            .HasForeignKey(v => v.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.AssetPropertyDefinition)
            .WithMany(p => p.PropertyValues)
            .HasForeignKey(v => v.AssetPropertyDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AssetLocationMappingConfiguration : IEntityTypeConfiguration<AssetLocationMapping>
{
    public void Configure(EntityTypeBuilder<AssetLocationMapping> builder)
    {
        builder.HasOne(m => m.Asset)
            .WithMany(a => a.LocationMappings)
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Location)
            .WithMany(l => l.AssetLocationMappings)
            .HasForeignKey(m => m.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VerificationLogConfiguration : IEntityTypeConfiguration<VerificationLog>
{
    public void Configure(EntityTypeBuilder<VerificationLog> builder)
    {
        builder.HasOne(v => v.Asset)
            .WithMany(a => a.VerificationLogs)
            .HasForeignKey(v => v.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.VerifiedByUser)
            .WithMany()
            .HasForeignKey(v => v.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Location)
            .WithMany()
            .HasForeignKey(v => v.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocationVerificationLogConfiguration : IEntityTypeConfiguration<LocationVerificationLog>
{
    public void Configure(EntityTypeBuilder<LocationVerificationLog> builder)
    {
        builder.HasOne(v => v.Location)
            .WithMany(l => l.VerificationLogs)
            .HasForeignKey(v => v.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.VerifiedByUser)
            .WithMany()
            .HasForeignKey(v => v.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class QrPrintLogConfiguration : IEntityTypeConfiguration<QrPrintLog>
{
    public void Configure(EntityTypeBuilder<QrPrintLog> builder)
    {
        builder.HasOne(q => q.Asset)
            .WithMany(a => a.QrPrintLogs)
            .HasForeignKey(q => q.AssetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(q => q.Location)
            .WithMany()
            .HasForeignKey(q => q.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(q => q.PrintedByUser)
            .WithMany()
            .HasForeignKey(q => q.PrintedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
