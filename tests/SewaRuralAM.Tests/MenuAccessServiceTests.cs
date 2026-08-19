using SewaRuralAM.Core.Entities;
using SewaRuralAM.Infrastructure.Services;
using Xunit;

namespace SewaRuralAM.Tests;

public class MenuAccessServiceTests
{
    [Fact]
    public async Task GetRightsAsync_SignedInViewer_ReturnsViewOnlyRights()
    {
        using var factory = new TestDbContextFactory();
        await using (var context = factory.CreateDbContext())
        {
            var role = new Role { RoleName = "Viewer" };
            context.Roles.Add(role);
            var user = new User { UserName = "nawaz", FullName = "Nawaz", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123") };
            context.Users.Add(user);
            var menu = new Menu { MenuName = "Assets", Route = "AssetListPage" };
            context.Menus.Add(menu);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            context.MenuRights.Add(new MenuRight { MenuId = menu.Id, RoleId = role.Id, CanView = true, CanAdd = false, CanEdit = false });
            await context.SaveChangesAsync();
        }

        var authService = new AuthService(factory, new FakeSessionStore());
        await authService.LoginAsync("nawaz", "Pass@123");

        var menuRightsService = new MenuRightsService(factory.CreateDbContext());
        var menuAccessService = new MenuAccessService(menuRightsService, authService);

        var rights = await menuAccessService.GetRightsAsync("AssetListPage");

        Assert.True(rights.CanView);
        Assert.False(rights.CanAdd);
        Assert.False(rights.CanEdit);
    }

    [Fact]
    public async Task GetRightsAsync_NotSignedIn_ReturnsAllFalse()
    {
        using var factory = new TestDbContextFactory();
        var authService = new AuthService(factory, new FakeSessionStore());
        var menuRightsService = new MenuRightsService(factory.CreateDbContext());
        var menuAccessService = new MenuAccessService(menuRightsService, authService);

        var rights = await menuAccessService.GetRightsAsync("AssetListPage");

        Assert.False(rights.CanView);
        Assert.False(rights.CanAdd);
        Assert.False(rights.CanEdit);
        Assert.False(rights.CanDelete);
    }
}
