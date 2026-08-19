using SewaRuralAM.Core.Entities;
using SewaRuralAM.Infrastructure.Services;
using Xunit;

namespace SewaRuralAM.Tests;

public class MenuRightsServiceTests
{
    [Fact]
    public async Task GetAuthorizedMenus_UserLevelDenial_OverridesRoleLevelGrant()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateDbContext();

        var role = new Role { RoleName = "Viewer" };
        context.Roles.Add(role);
        var user = new User { UserName = "bob", FullName = "Bob", PasswordHash = "x" };
        context.Users.Add(user);
        var menu = new Menu { MenuName = "Reports", Route = "ReportsPage" };
        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        // Role grants View; user-level override explicitly denies it for this one user.
        context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = role.Id, CanView = true });
        context.MenuRights.Add(new MenuRight { MenuId = menu.Id, UserId = user.Id, CanView = false });
        await context.SaveChangesAsync();

        var service = new MenuRightsService(context);
        var authorizedMenus = await service.GetAuthorizedMenusAsync(user.Id);

        Assert.DoesNotContain(authorizedMenus, m => m.Id == menu.Id);
    }

    [Fact]
    public async Task GetAuthorizedMenus_UserLevelGrant_OverridesRoleLevelDenial()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateDbContext();

        var role = new Role { RoleName = "Viewer" };
        context.Roles.Add(role);
        var user = new User { UserName = "alice", FullName = "Alice", PasswordHash = "x" };
        context.Users.Add(user);
        var menu = new Menu { MenuName = "Administration", Route = "UserManagementPage" };
        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        // Role denies View; user-level override explicitly grants it for this one user.
        context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = role.Id, CanView = false });
        context.MenuRights.Add(new MenuRight { MenuId = menu.Id, UserId = user.Id, CanView = true });
        await context.SaveChangesAsync();

        var service = new MenuRightsService(context);
        var authorizedMenus = await service.GetAuthorizedMenusAsync(user.Id);

        Assert.Contains(authorizedMenus, m => m.Id == menu.Id);
    }

    [Fact]
    public async Task GetAuthorizedMenus_NoRights_MenuNotVisible()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateDbContext();

        var user = new User { UserName = "nobody", FullName = "Nobody", PasswordHash = "x" };
        context.Users.Add(user);
        var menu = new Menu { MenuName = "Reports", Route = "ReportsPage" };
        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        var service = new MenuRightsService(context);
        var authorizedMenus = await service.GetAuthorizedMenusAsync(user.Id);

        Assert.Empty(authorizedMenus);
    }
}
