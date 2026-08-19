using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Infrastructure.Data;

namespace SewaRuralAM.Infrastructure.Services;

// Registered as a singleton so CurrentUser survives across page navigation; it creates a
// short-lived AppDbContext per operation via the factory rather than holding one long-term.
public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ISessionStore _sessionStore;

    public User? CurrentUser { get; private set; }

    public AuthService(IDbContextFactory<AppDbContext> contextFactory, ISessionStore sessionStore)
    {
        _contextFactory = contextFactory;
        _sessionStore = sessionStore;
    }

    public void SetCurrentUser(User? user) => CurrentUser = user;

    public async Task<User?> LoginAsync(string userName, string password, bool rememberMe = false)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        user.LastLoginDate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        CurrentUser = user;

        // A signed-in session stays active until explicit Logout, regardless of Remember Me;
        // Remember Me is kept only as a UI affordance for now.
        _sessionStore.SaveUserId(user.Id);

        return user;
    }

    public async Task<User?> RestoreSessionAsync()
    {
        var userId = _sessionStore.GetUserId();
        if (userId is null)
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive);

        CurrentUser = user;
        if (user is null)
            _sessionStore.Clear();

        return user;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        _sessionStore.Clear();
        return Task.CompletedTask;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users.FindAsync(userId);
        if (user is null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await context.SaveChangesAsync();

        if (CurrentUser?.Id == userId)
            CurrentUser = user;

        return true;
    }

    public async Task<string?> GenerateResetTokenAsync(string userNameOrEmail)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(
            u => u.UserName == userNameOrEmail || u.Email == userNameOrEmail);
        if (user is null)
            return null;

        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken);
        if (user is null || user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await context.SaveChangesAsync();
        return true;
    }
}
