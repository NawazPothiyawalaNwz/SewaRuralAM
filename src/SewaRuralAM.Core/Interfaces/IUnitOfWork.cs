using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<Menu> Menus { get; }
    IRepository<MenuRight> MenuRights { get; }
    IRepository<Location> Locations { get; }
    IRepository<AssetCategory> AssetCategories { get; }
    IRepository<Asset> Assets { get; }
    IRepository<AssetPropertyDefinition> AssetPropertyDefinitions { get; }
    IRepository<AssetPropertyValue> AssetPropertyValues { get; }
    IRepository<AssetLocationMapping> AssetLocationMappings { get; }
    IRepository<VerificationLog> VerificationLogs { get; }
    IRepository<LocationVerificationLog> LocationVerificationLogs { get; }
    IRepository<QrPrintLog> QrPrintLogs { get; }

    Task<int> SaveChangesAsync();
}
