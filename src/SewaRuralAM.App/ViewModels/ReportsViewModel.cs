using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.App.Services;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ReportsViewModel(IUnitOfWork unitOfWork, IPdfService pdfService, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
        _toastService = toastService;
        Title = "Reports";
    }

    [RelayCommand]
    private async Task GenerateAssetRegisterAsync()
    {
        var assets = await _unitOfWork.Assets.Query().Include(a => a.AssetCategory).ToListAsync();

        if (assets.Count == 0)
        {
            StatusMessage = "No assets to report on yet.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        var pdfBytes = _pdfService.GenerateAssetRegisterReport(assets);
        var filePath = await PdfFileHelper.SaveAndOpenAsync(pdfBytes, "AssetRegister");

        StatusMessage = $"Report saved to {filePath}";
        _toastService.Show("Asset Register PDF generated.", ToastKind.Success);
    }

    [RelayCommand]
    private async Task GenerateAssetVerificationReportAsync()
    {
        var logs = await _unitOfWork.VerificationLogs.Query()
            .Include(l => l.Asset)
            .Include(l => l.VerifiedByUser)
            .Include(l => l.Location)
            .ToListAsync();

        var pdfBytes = _pdfService.GenerateAssetVerificationReport(logs);
        var filePath = await PdfFileHelper.SaveAndOpenAsync(pdfBytes, "AssetVerificationReport");

        StatusMessage = $"Report saved to {filePath}";
        _toastService.Show("Asset Verification Report generated.", ToastKind.Success);
    }

    [RelayCommand]
    private async Task GenerateLocationVerificationReportAsync()
    {
        var logs = await _unitOfWork.LocationVerificationLogs.Query()
            .Include(l => l.Location)
            .Include(l => l.VerifiedByUser)
            .ToListAsync();

        var pdfBytes = _pdfService.GenerateLocationVerificationReport(logs);
        var filePath = await PdfFileHelper.SaveAndOpenAsync(pdfBytes, "LocationVerificationReport");

        StatusMessage = $"Report saved to {filePath}";
        _toastService.Show("Location Verification Report generated.", ToastKind.Success);
    }

    [RelayCommand]
    private static async Task OpenAssetQrPrintAsync() =>
        await Shell.Current.GoToAsync(nameof(Views.AssetQrPrintPage));

    [RelayCommand]
    private static async Task OpenLocationQrPrintAsync() =>
        await Shell.Current.GoToAsync(nameof(Views.LocationQrPrintPage));
}
