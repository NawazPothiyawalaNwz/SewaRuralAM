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
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
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
            context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = viewerRole.Id, CanView = true });
        }
        await context.SaveChangesAsync();

        var headOffice = new Location { LocationCode = "LOC-HO", LocationName = "Head Office", LevelNo = 1 };
        context.Locations.Add(headOffice);
        await context.SaveChangesAsync();

        var buildingA = new Location { LocationCode = "LOC-HO-BA", LocationName = "Building A", LevelNo = 2, ParentLocationId = headOffice.Id };
        context.Locations.Add(buildingA);
        await context.SaveChangesAsync();

        var floor1 = new Location { LocationCode = "LOC-HO-BA-F1", LocationName = "Floor 1", LevelNo = 3, ParentLocationId = buildingA.Id };
        context.Locations.Add(floor1);
        await context.SaveChangesAsync();

        var room101 = new Location { LocationCode = "LOC-HO-BA-F1-R101", LocationName = "Room 101", LevelNo = 4, ParentLocationId = floor1.Id };
        context.Locations.Add(room101);
        await context.SaveChangesAsync();

        var rack1 = new Location { LocationCode = "LOC-HO-BA-F1-R101-RK1", LocationName = "Rack 1", LevelNo = 5, ParentLocationId = room101.Id };
        context.Locations.Add(rack1);
        await context.SaveChangesAsync();

        var shelfA = new Location { LocationCode = "LOC-HO-BA-F1-R101-RK1-SA", LocationName = "Shelf A", LevelNo = 6, ParentLocationId = rack1.Id };
        context.Locations.Add(shelfA);
        await context.SaveChangesAsync();

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

        var laptop = new Asset
        {
            AssetCode = "AST-0001",
            AssetName = "Dell Latitude 5440",
            AssetCategoryId = computers.Id,
            AssetType = "Laptop",
            Brand = "Dell",
            Model = "Latitude 5440",
            SerialNumber = "DL5440-001",
            PurchaseDate = DateTime.UtcNow.AddYears(-1),
            PurchaseCost = 75000,
            Vendor = "Dell India",
            WarrantyExpiry = DateTime.UtcNow.AddYears(2),
            Status = AssetStatus.Active
        };
        context.Assets.Add(laptop);
        await context.SaveChangesAsync();

        context.AssetLocationMappings.Add(new AssetLocationMapping
        {
            AssetId = laptop.Id,
            LocationId = shelfA.Id,
            AssignedDate = DateTime.UtcNow,
            IsCurrent = true
        });
        await context.SaveChangesAsync();
    }
}
