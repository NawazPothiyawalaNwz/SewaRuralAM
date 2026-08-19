using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuRight> MenuRights => Set<MenuRight>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetPropertyDefinition> AssetPropertyDefinitions => Set<AssetPropertyDefinition>();
    public DbSet<AssetPropertyValue> AssetPropertyValues => Set<AssetPropertyValue>();
    public DbSet<AssetLocationMapping> AssetLocationMappings => Set<AssetLocationMapping>();
    public DbSet<VerificationLog> VerificationLogs => Set<VerificationLog>();
    public DbSet<LocationVerificationLog> LocationVerificationLogs => Set<LocationVerificationLog>();
    public DbSet<QrPrintLog> QrPrintLogs => Set<QrPrintLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
