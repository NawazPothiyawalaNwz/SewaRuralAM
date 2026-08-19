using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Enums;

namespace SewaRuralAM.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync())
            return;

        var adminRole = new Role { RoleName = "Administrator", Description = "Full system access" };
        var managerRole = new Role { RoleName = "Manager", Description = "Manage assets and locations" };
        var viewerRole = new Role { RoleName = "Viewer", Description = "Read-only access" };
        context.Roles.AddRange(adminRole, managerRole, viewerRole);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            UserName = "admin",
            FullName = "System Administrator",
            Email = "admin@sewarural.org",
            PhoneNumber = "0000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
        };
        var managerUser = new User
        {
            UserName = "manager",
            FullName = "Priya Sharma",
            Email = "priya.sharma@sewarural.org",
            PhoneNumber = "9000000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123")
        };
        var viewerUser = new User
        {
            UserName = "viewer",
            FullName = "Ramesh Patel",
            Email = "ramesh.patel@sewarural.org",
            PhoneNumber = "9000000002",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer@123")
        };
        context.Users.AddRange(adminUser, managerUser, viewerUser);
        await context.SaveChangesAsync();

        context.UserRoles.AddRange(
            new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
            new UserRole { UserId = managerUser.Id, RoleId = managerRole.Id },
            new UserRole { UserId = viewerUser.Id, RoleId = viewerRole.Id });
        await context.SaveChangesAsync();

        var dashboard = new Menu { MenuName = "Dashboard", Icon = "home.png", Route = "DashboardPage", DisplayOrder = 1 };
        var assetsMenu = new Menu { MenuName = "Assets", Icon = "assets.png", Route = "AssetListPage", DisplayOrder = 2 };
        var locationsMenu = new Menu { MenuName = "Locations", Icon = "locations.png", Route = "LocationTreePage", DisplayOrder = 3 };
        var qrMenu = new Menu { MenuName = "QR Scanner", Icon = "qr.png", Route = "QrScannerPage", DisplayOrder = 4 };
        var reportsMenu = new Menu { MenuName = "Reports", Icon = "reports.png", Route = "ReportsPage", DisplayOrder = 5 };
        var adminMenu = new Menu { MenuName = "Administration", Icon = "admin.png", DisplayOrder = 6 };
        context.Menus.AddRange(dashboard, assetsMenu, locationsMenu, qrMenu, reportsMenu, adminMenu);
        await context.SaveChangesAsync();

        var usersSubMenu = new Menu { MenuName = "Users & Roles", Icon = "users.png", Route = "UserListPage", DisplayOrder = 1, ParentMenuId = adminMenu.Id };
        var menuRightsSubMenu = new Menu { MenuName = "Menu Rights", Icon = "rights.png", Route = "MenuRightsPage", DisplayOrder = 2, ParentMenuId = adminMenu.Id };
        context.Menus.AddRange(usersSubMenu, menuRightsSubMenu);
        await context.SaveChangesAsync();

        var allMenus = new[] { dashboard, assetsMenu, locationsMenu, qrMenu, reportsMenu, adminMenu, usersSubMenu, menuRightsSubMenu };
        foreach (var menu in allMenus)
        {
            context.MenuRights.Add(new MenuRight
            {
                MenuId = menu.Id,
                RoleId = adminRole.Id,
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                CanPrint = true,
                CanExport = true,
                CanQrPrint = true
            });
        }
        foreach (var menu in new[] { dashboard, assetsMenu, locationsMenu, qrMenu, reportsMenu })
        {
            context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = managerRole.Id, CanView = true, CanAdd = true, CanEdit = true, CanPrint = true, CanQrPrint = true });
            context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = viewerRole.Id, CanView = true });
        }
        await context.SaveChangesAsync();

        // Branch 1: Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A / Shelf B
        var headOffice = await AddLocationAsync(context, "LOC-HO", "Head Office", 1, null);
        var buildingA = await AddLocationAsync(context, "LOC-HO-BA", "Building A", 2, headOffice.Id);
        var floor1 = await AddLocationAsync(context, "LOC-HO-BA-F1", "Floor 1", 3, buildingA.Id);
        var room101 = await AddLocationAsync(context, "LOC-HO-BA-F1-R101", "Room 101", 4, floor1.Id);
        var rack1 = await AddLocationAsync(context, "LOC-HO-BA-F1-R101-RK1", "Rack 1", 5, room101.Id);
        var shelfA = await AddLocationAsync(context, "LOC-HO-BA-F1-R101-RK1-SA", "Shelf A", 6, rack1.Id);
        var shelfB = await AddLocationAsync(context, "LOC-HO-BA-F1-R101-RK1-SB", "Shelf B", 6, rack1.Id);

        // Branch 1b: Head Office > Building A > Floor 2 > Room 201 > Rack 1 > Shelf A
        var floor2 = await AddLocationAsync(context, "LOC-HO-BA-F2", "Floor 2", 3, buildingA.Id);
        var room201 = await AddLocationAsync(context, "LOC-HO-BA-F2-R201", "Room 201", 4, floor2.Id);
        var rack201 = await AddLocationAsync(context, "LOC-HO-BA-F2-R201-RK1", "Rack 1", 5, room201.Id);
        var shelfR201A = await AddLocationAsync(context, "LOC-HO-BA-F2-R201-RK1-SA", "Shelf A", 6, rack201.Id);

        // Branch 2: Warehouse > Block C > Floor 1 > Room 10 > Rack 2 > Shelf C
        var warehouse = await AddLocationAsync(context, "LOC-WH", "Warehouse", 1, null);
        var blockC = await AddLocationAsync(context, "LOC-WH-BC", "Block C", 2, warehouse.Id);
        var whFloor1 = await AddLocationAsync(context, "LOC-WH-BC-F1", "Floor 1", 3, blockC.Id);
        var room10 = await AddLocationAsync(context, "LOC-WH-BC-F1-R10", "Room 10", 4, whFloor1.Id);
        var rack2 = await AddLocationAsync(context, "LOC-WH-BC-F1-R10-RK2", "Rack 2", 5, room10.Id);
        var shelfC = await AddLocationAsync(context, "LOC-WH-BC-F1-R10-RK2-SC", "Shelf C", 6, rack2.Id);

        var computers = new AssetCategory { CategoryName = "Computers", Description = "Desktops and laptops" };
        var furniture = new AssetCategory { CategoryName = "Furniture", Description = "Office furniture" };
        var vehicles = new AssetCategory { CategoryName = "Vehicles", Description = "Vehicles and transport" };
        context.AssetCategories.AddRange(computers, furniture, vehicles);
        await context.SaveChangesAsync();

        context.AssetPropertyDefinitions.AddRange(
            new AssetPropertyDefinition { AssetCategoryId = computers.Id, PropertyName = "CPU", DataType = PropertyDataType.Text, DisplayOrder = 1 },
            new AssetPropertyDefinition { AssetCategoryId = computers.Id, PropertyName = "RAM", DataType = PropertyDataType.Text, DisplayOrder = 2 },
            new AssetPropertyDefinition { AssetCategoryId = computers.Id, PropertyName = "HDD", DataType = PropertyDataType.Text, DisplayOrder = 3 },
            new AssetPropertyDefinition { AssetCategoryId = vehicles.Id, PropertyName = "Registration Number", DataType = PropertyDataType.Text, IsRequired = true, DisplayOrder = 1 },
            new AssetPropertyDefinition { AssetCategoryId = vehicles.Id, PropertyName = "Engine Number", DataType = PropertyDataType.Text, DisplayOrder = 2 },
            new AssetPropertyDefinition { AssetCategoryId = furniture.Id, PropertyName = "Material", DataType = PropertyDataType.Text, DisplayOrder = 1 },
            new AssetPropertyDefinition { AssetCategoryId = furniture.Id, PropertyName = "Color", DataType = PropertyDataType.Text, DisplayOrder = 2 }
        );
        await context.SaveChangesAsync();

        var laptop = await AddAssetAsync(context, "AST-0001", "Dell Latitude 5440", computers.Id, "Dell", "Latitude 5440", "DL5440-001", 75000, AssetStatus.Active, shelfA.Id);
        var desktop = await AddAssetAsync(context, "AST-0002", "HP EliteDesk 800", computers.Id, "HP", "EliteDesk 800 G9", "HPED800-014", 58000, AssetStatus.Active, shelfA.Id);
        var printer = await AddAssetAsync(context, "AST-0003", "Canon LBP2900 Printer", computers.Id, "Canon", "LBP2900", "CN2900-221", 9500, AssetStatus.UnderRepair, shelfB.Id);
        var chair = await AddAssetAsync(context, "AST-0004", "Executive Office Chair", furniture.Id, "Featherlite", "Ergo-200", "FL-EG200-09", 12000, AssetStatus.Active, shelfR201A.Id);
        var desk = await AddAssetAsync(context, "AST-0005", "Office Workstation Desk", furniture.Id, "Godrej", "WS-4Seater", "GJ-WS4-33", 26000, AssetStatus.Active, shelfR201A.Id);
        var bike = await AddAssetAsync(context, "AST-0006", "Field Visit Motorcycle", vehicles.Id, "Honda", "Shine 125", "HD-SH125-77", 85000, AssetStatus.Active, shelfC.Id);
        var jeep = await AddAssetAsync(context, "AST-0007", "Mobile Health Van", vehicles.Id, "Mahindra", "Bolero Camper", "MH-BC-05", 950000, AssetStatus.Active, shelfC.Id);
        var oldLaptop = await AddAssetAsync(context, "AST-0008", "Lenovo ThinkPad (Retired)", computers.Id, "Lenovo", "ThinkPad T480", "LN-T480-02", 45000, AssetStatus.Disposed, shelfB.Id);

        // A handful of verified assets/locations with realistic verification history for Reports testing.
        await VerifyAssetAsync(context, laptop, shelfA, adminUser, "Checked physically, condition good.", DateTime.UtcNow.AddDays(-10));
        await VerifyAssetAsync(context, desktop, shelfA, managerUser, "Verified during monthly audit.", DateTime.UtcNow.AddDays(-3));
        await VerifyAssetAsync(context, bike, shelfC, managerUser, "Odometer and condition checked.", DateTime.UtcNow.AddDays(-1));

        await VerifyLocationAsync(context, shelfA, adminUser, "Shelf inspected, all items accounted for.", DateTime.UtcNow.AddDays(-10));
        await VerifyLocationAsync(context, shelfC, managerUser, "Warehouse shelf inspected.", DateTime.UtcNow.AddDays(-1));
    }

    private static async Task<Location> AddLocationAsync(AppDbContext context, string code, string name, int level, int? parentId)
    {
        var location = new Location { LocationCode = code, LocationName = name, LevelNo = level, ParentLocationId = parentId };
        context.Locations.Add(location);
        await context.SaveChangesAsync();
        return location;
    }

    private static async Task<Asset> AddAssetAsync(AppDbContext context, string code, string name, int categoryId,
        string brand, string model, string serialNumber, decimal cost, AssetStatus status, int locationId)
    {
        var asset = new Asset
        {
            AssetCode = code,
            AssetName = name,
            AssetCategoryId = categoryId,
            Brand = brand,
            Model = model,
            SerialNumber = serialNumber,
            PurchaseDate = DateTime.UtcNow.AddMonths(-Random.Shared.Next(2, 30)),
            PurchaseCost = cost,
            Vendor = $"{brand} India",
            WarrantyExpiry = DateTime.UtcNow.AddYears(1),
            Status = status
        };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        context.AssetLocationMappings.Add(new AssetLocationMapping
        {
            AssetId = asset.Id,
            LocationId = locationId,
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true
        });
        await context.SaveChangesAsync();

        return asset;
    }

    private static async Task VerifyAssetAsync(AppDbContext context, Asset asset, Location location, User user, string remarks, DateTime verifiedDate)
    {
        asset.IsVerified = true;
        asset.LastVerifiedDate = verifiedDate;
        context.Assets.Update(asset);

        context.VerificationLogs.Add(new VerificationLog
        {
            AssetId = asset.Id,
            VerifiedByUserId = user.Id,
            VerifiedDate = verifiedDate,
            LocationId = location.Id,
            Remarks = remarks
        });
        await context.SaveChangesAsync();
    }

    private static async Task VerifyLocationAsync(AppDbContext context, Location location, User user, string remarks, DateTime verifiedDate)
    {
        location.IsVerified = true;
        location.LastVerifiedDate = verifiedDate;
        context.Locations.Update(location);

        context.LocationVerificationLogs.Add(new LocationVerificationLog
        {
            LocationId = location.Id,
            VerifiedByUserId = user.Id,
            VerifiedDate = verifiedDate,
            Remarks = remarks
        });
        await context.SaveChangesAsync();
    }
}
