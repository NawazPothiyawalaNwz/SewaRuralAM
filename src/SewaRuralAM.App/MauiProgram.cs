using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SewaRuralAM.App.Views;
using SewaRuralAM.App.ViewModels;
using SewaRuralAM.App.Services;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Infrastructure.Data;
using SewaRuralAM.Infrastructure.Repositories;
using SewaRuralAM.Infrastructure.Services;
using ZXing.Net.Maui.Controls;

namespace SewaRuralAM.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
                fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "sewarural.db");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddTransient<IMenuRightsService, MenuRightsService>();
        builder.Services.AddSingleton<ISessionStore, PreferencesSessionStore>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
        builder.Services.AddSingleton<IPdfService, PdfService>();
        builder.Services.AddSingleton<IToastService, ToastService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<AssetListViewModel>();
        builder.Services.AddTransient<AssetDetailViewModel>();
        builder.Services.AddTransient<LocationTreeViewModel>();
        builder.Services.AddTransient<LocationEditViewModel>();
        builder.Services.AddTransient<QrScannerViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserEditViewModel>();
        builder.Services.AddTransient<MenuRightsViewModel>();
        builder.Services.AddTransient<AssetQrPrintViewModel>();
        builder.Services.AddTransient<LocationQrPrintViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<ChangePasswordPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AssetListPage>();
        builder.Services.AddTransient<AssetDetailPage>();
        builder.Services.AddTransient<LocationTreePage>();
        builder.Services.AddTransient<LocationEditPage>();
        builder.Services.AddTransient<QrScannerPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserEditPage>();
        builder.Services.AddTransient<MenuRightsPage>();
        builder.Services.AddTransient<AssetQrPrintPage>();
        builder.Services.AddTransient<LocationQrPrintPage>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var context = contextFactory.CreateDbContext();
            DbInitializer.SeedAsync(context).GetAwaiter().GetResult();
        }

        return app;
    }
}
