using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Infrastructure.Data.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasIndex(l => l.LocationCode).IsUnique();
        builder.Property(l => l.LocationCode).IsRequired().HasMaxLength(50);
        builder.Property(l => l.LocationName).IsRequired().HasMaxLength(200);

        builder.HasOne(l => l.ParentLocation)
            .WithMany(l => l.ChildLocations)
            .HasForeignKey(l => l.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
