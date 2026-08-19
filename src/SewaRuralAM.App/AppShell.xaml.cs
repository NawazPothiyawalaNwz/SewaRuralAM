using Microsoft.Extensions.DependencyInjection;
using SewaRuralAM.App.Views;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App;

public partial class AppShell : Shell
{
    private readonly IAuthService _authService;
    private readonly IMenuRightsService _menuRightsService;

    public AppShell()
    {
        InitializeComponent();
        var services = IPlatformApplication.Current!.Services;
        _authService = services.GetRequiredService<IAuthService>();
        _menuRightsService = services.GetRequiredService<IMenuRightsService>();

        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(AssetDetailPage), typeof(AssetDetailPage));
        Routing.RegisterRoute(nameof(LocationEditPage), typeof(LocationEditPage));
        Routing.RegisterRoute(nameof(UserEditPage), typeof(UserEditPage));
        Routing.RegisterRoute(nameof(AssetQrPrintPage), typeof(AssetQrPrintPage));
        Routing.RegisterRoute(nameof(LocationQrPrintPage), typeof(LocationQrPrintPage));

        _ = TryRestoreSessionAsync();
    }

    private async Task TryRestoreSessionAsync()
    {
        var user = await _authService.RestoreSessionAsync();
        if (user is not null)
        {
            await OnSignedInAsync(user);
        }
        else
        {
            await GoToAsync("//LoginPage");
        }
    }

    public async Task OnSignedInAsync(User user)
    {
        UpdateUserChrome(user);
        await ApplyMenuVisibilityAsync(user.Id);
        await GoToAsync("//DashboardPage");
    }

    private void UpdateUserChrome(User user)
    {
        UserNameLabel.Text = user.FullName;
        UserRoleLabel.Text = user.UserName;

        var initials = string.Concat(user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(p => char.ToUpperInvariant(p[0])));
        UserInitialsLabel.Text = string.IsNullOrEmpty(initials) ? "?" : initials;
    }

    private async Task ApplyMenuVisibilityAsync(int userId)
    {
        var authorizedMenus = await _menuRightsService.GetAuthorizedMenusAsync(userId);
        var routes = authorizedMenus.Where(m => m.Route is not null).Select(m => m.Route!).ToHashSet();

        SetFlyoutItemIsVisible(DashboardFlyoutItem, routes.Contains("DashboardPage"));
        SetFlyoutItemIsVisible(AssetsFlyoutItem, routes.Contains("AssetListPage"));
        SetFlyoutItemIsVisible(LocationsFlyoutItem, routes.Contains("LocationTreePage"));
        SetFlyoutItemIsVisible(QrFlyoutItem, routes.Contains("QrScannerPage"));
        SetFlyoutItemIsVisible(ReportsFlyoutItem, routes.Contains("ReportsPage"));

        var canSeeUsers = routes.Contains("UserListPage");
        var canSeeMenuRights = routes.Contains("MenuRightsPage");
        SetFlyoutItemIsVisible(AdminFlyoutItem, canSeeUsers || canSeeMenuRights);
        UsersTab.IsVisible = canSeeUsers;
        MenuRightsTab.IsVisible = canSeeMenuRights;
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await _authService.LogoutAsync();

        UserNameLabel.Text = "Not signed in";
        UserRoleLabel.Text = string.Empty;
        UserInitialsLabel.Text = "?";

        await GoToAsync("//LoginPage");
    }
}
