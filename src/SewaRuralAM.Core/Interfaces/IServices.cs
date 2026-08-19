using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Core.Interfaces;

public interface IAuthService
{
    Task<User?> LoginAsync(string userName, string password, bool rememberMe = false);
    Task<User?> RestoreSessionAsync();
    Task LogoutAsync();
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<string?> GenerateResetTokenAsync(string userNameOrEmail);
    Task<bool> ResetPasswordAsync(string resetToken, string newPassword);
    User? CurrentUser { get; }
    void SetCurrentUser(User? user);
}

// Persists the "Remember Me" session across app restarts. Implemented per-platform in the
// App project since Infrastructure has no dependency on MAUI's storage APIs.
public interface ISessionStore
{
    void SaveUserId(int userId);
    int? GetUserId();
    void Clear();
}

public interface IMenuRightsService
{
    Task<List<Menu>> GetAuthorizedMenusAsync(int userId);
    Task<MenuRight?> GetEffectiveRightsAsync(int userId, int menuId);
}

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content, int pixelsPerModule = 10);
}

public interface IPdfService
{
    byte[] GenerateAssetQrSheet(IEnumerable<(string AssetCode, string AssetName, byte[] QrImage)> assets);
    byte[] GenerateAssetRegisterReport(IEnumerable<Asset> assets);
    byte[] GenerateAssetVerificationReport(IEnumerable<VerificationLog> logs);
    byte[] GenerateLocationVerificationReport(IEnumerable<LocationVerificationLog> logs);
}

public enum ToastKind
{
    Success,
    Error,
    Info
}

public interface IToastService
{
    void Show(string message, ToastKind kind = ToastKind.Info);
}
