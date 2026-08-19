using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Infrastructure.Data;

namespace SewaRuralAM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<UserRole>? _userRoles;
    private IRepository<Menu>? _menus;
    private IRepository<MenuRight>? _menuRights;
    private IRepository<Location>? _locations;
    private IRepository<AssetCategory>? _assetCategories;
    private IRepository<Asset>? _assets;
    private IRepository<AssetPropertyDefinition>? _assetPropertyDefinitions;
    private IRepository<AssetPropertyValue>? _assetPropertyValues;
    private IRepository<AssetLocationMapping>? _assetLocationMappings;
    private IRepository<VerificationLog>? _verificationLogs;
    private IRepository<LocationVerificationLog>? _locationVerificationLogs;
    private IRepository<QrPrintLog>? _qrPrintLogs;

    public UnitOfWork(IDbContextFactory<AppDbContext> contextFactory)
    {
        _context = contextFactory.CreateDbContext();
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);
    public IRepository<Menu> Menus => _menus ??= new Repository<Menu>(_context);
    public IRepository<MenuRight> MenuRights => _menuRights ??= new Repository<MenuRight>(_context);
    public IRepository<Location> Locations => _locations ??= new Repository<Location>(_context);
    public IRepository<AssetCategory> AssetCategories => _assetCategories ??= new Repository<AssetCategory>(_context);
    public IRepository<Asset> Assets => _assets ??= new Repository<Asset>(_context);
    public IRepository<AssetPropertyDefinition> AssetPropertyDefinitions => _assetPropertyDefinitions ??= new Repository<AssetPropertyDefinition>(_context);
    public IRepository<AssetPropertyValue> AssetPropertyValues => _assetPropertyValues ??= new Repository<AssetPropertyValue>(_context);
    public IRepository<AssetLocationMapping> AssetLocationMappings => _assetLocationMappings ??= new Repository<AssetLocationMapping>(_context);
    public IRepository<VerificationLog> VerificationLogs => _verificationLogs ??= new Repository<VerificationLog>(_context);
    public IRepository<LocationVerificationLog> LocationVerificationLogs => _locationVerificationLogs ??= new Repository<LocationVerificationLog>(_context);
    public IRepository<QrPrintLog> QrPrintLogs => _qrPrintLogs ??= new Repository<QrPrintLog>(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
