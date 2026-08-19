using SewaRuralAM.Core.Entities;
using SewaRuralAM.Infrastructure.Services;
using Xunit;

namespace SewaRuralAM.Tests;

public class AuthServiceTests
{
    private static async Task<TestDbContextFactory> SeedUserAsync(string userName, string password)
    {
        var factory = new TestDbContextFactory();
        await using var context = factory.CreateDbContext();

        context.Users.Add(new User
        {
            UserName = userName,
            FullName = "Test User",
            Email = "test@sewarural.org",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });
        await context.SaveChangesAsync();

        return factory;
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsUserAndSetsCurrentUser()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var authService = new AuthService(factory, new FakeSessionStore());

        var user = await authService.LoginAsync("admin", "Admin@123");

        Assert.NotNull(user);
        Assert.Equal("admin", authService.CurrentUser?.UserName);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var authService = new AuthService(factory, new FakeSessionStore());

        var user = await authService.LoginAsync("admin", "WrongPassword");

        Assert.Null(user);
        Assert.Null(authService.CurrentUser);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_UpdatesHash()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var authService = new AuthService(factory, new FakeSessionStore());
        var user = await authService.LoginAsync("admin", "Admin@123");

        var changed = await authService.ChangePasswordAsync(user!.Id, "Admin@123", "NewPass@123");
        Assert.True(changed);

        var reLogin = await authService.LoginAsync("admin", "NewPass@123");
        Assert.NotNull(reLogin);
    }

    [Fact]
    public async Task RestoreSessionAsync_AfterLogin_ReturnsSameUser()
    {
        // A signed-in session stays active across app restarts until explicit Logout,
        // regardless of the Remember Me checkbox (see AuthService.LoginAsync).
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var sessionStore = new FakeSessionStore();
        var authService = new AuthService(factory, sessionStore);

        await authService.LoginAsync("admin", "Admin@123", rememberMe: false);

        var restoredService = new AuthService(factory, sessionStore);
        var restoredUser = await restoredService.RestoreSessionAsync();

        Assert.NotNull(restoredUser);
        Assert.Equal("admin", restoredUser!.UserName);
    }

    [Fact]
    public async Task RestoreSessionAsync_WithNoPriorLogin_ReturnsNull()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var sessionStore = new FakeSessionStore();
        var authService = new AuthService(factory, sessionStore);

        var restoredUser = await authService.RestoreSessionAsync();

        Assert.Null(restoredUser);
    }

    [Fact]
    public async Task LogoutAsync_ClearsCurrentUserAndSession()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var sessionStore = new FakeSessionStore();
        var authService = new AuthService(factory, sessionStore);
        await authService.LoginAsync("admin", "Admin@123");

        await authService.LogoutAsync();

        Assert.Null(authService.CurrentUser);
        Assert.Null(sessionStore.GetUserId());
    }

    [Fact]
    public async Task RestoreSessionAsync_AfterLogout_ReturnsNull()
    {
        using var factory = await SeedUserAsync("admin", "Admin@123");
        var sessionStore = new FakeSessionStore();
        var authService = new AuthService(factory, sessionStore);
        await authService.LoginAsync("admin", "Admin@123");
        await authService.LogoutAsync();

        var restoredService = new AuthService(factory, sessionStore);
        var restoredUser = await restoredService.RestoreSessionAsync();

        Assert.Null(restoredUser);
    }
}
