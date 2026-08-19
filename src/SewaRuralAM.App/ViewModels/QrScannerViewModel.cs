using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Core.Services;
using ZXing.Net.Maui;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

public partial class QrScannerViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IToastService _toastService;
    private readonly IMenuAccessService _menuAccessService;

    [ObservableProperty]
    private bool isScanning = true;

    [ObservableProperty]
    private Asset? scannedAsset;

    [ObservableProperty]
    private Location? scannedLocation;

    [ObservableProperty]
    private string scannedLocationChain = string.Empty;

    [ObservableProperty]
    private string currentLocationName = string.Empty;

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public bool HasResult => ScannedAsset is not null || ScannedLocation is not null;

    public QrScannerViewModel(IUnitOfWork unitOfWork, IAuthService authService, IToastService toastService, IMenuAccessService menuAccessService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _toastService = toastService;
        _menuAccessService = menuAccessService;
        Title = "QR Verification";
    }

    public async Task HandleDetectedBarcodesAsync(IReadOnlyList<BarcodeResult> results)
    {
        if (!IsScanning || results.Count == 0) return;

        var value = results[0].Value;
        var parts = value.Split('|');

        if (parts.Length < 2 || !int.TryParse(parts[1], out var id))
        {
            StatusMessage = "Unrecognized QR code.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        if (parts[0] == "ASSET")
        {
            await LoadAssetAsync(id);
        }
        else if (parts[0] == "LOCATION")
        {
            await LoadLocationAsync(id);
        }
        else
        {
            StatusMessage = "Unrecognized QR code.";
            _toastService.Show(StatusMessage, ToastKind.Error);
        }
    }

    private async Task LoadAssetAsync(int assetId)
    {
        IsScanning = false;

        var asset = await _unitOfWork.Assets.Query()
            .Include(a => a.AssetCategory)
            .Include(a => a.PropertyValues).ThenInclude(v => v.AssetPropertyDefinition)
            .Include(a => a.LocationMappings).ThenInclude(m => m.Location)
            .FirstOrDefaultAsync(a => a.Id == assetId);

        if (asset is null)
        {
            StatusMessage = "Asset not found.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        ScannedAsset = asset;
        CurrentLocationName = asset.LocationMappings.FirstOrDefault(m => m.IsCurrent)?.Location.LocationName ?? "Not assigned";
        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(HasResult));
    }

    private async Task LoadLocationAsync(int locationId)
    {
        IsScanning = false;

        var allLocations = await _unitOfWork.Locations.GetAllAsync();
        var location = allLocations.FirstOrDefault(l => l.Id == locationId);

        if (location is null)
        {
            StatusMessage = "Location not found.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        ScannedLocation = location;
        ScannedLocationChain = LocationChainHelper.BuildChain(location, LocationChainHelper.ToLookup(allLocations));
        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(HasResult));
    }

    [RelayCommand]
    private async Task ConfirmVerificationAsync()
    {
        if (_authService.CurrentUser is null) return;

        var rights = await _menuAccessService.GetRightsAsync("QrScannerPage");
        if (!rights.CanEdit)
        {
            StatusMessage = "You don't have permission to verify.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        if (ScannedAsset is not null)
        {
            await ConfirmAssetVerificationAsync(ScannedAsset);
        }
        else if (ScannedLocation is not null)
        {
            await ConfirmLocationVerificationAsync(ScannedLocation);
        }
    }

    private async Task ConfirmAssetVerificationAsync(Asset scannedAsset)
    {
        var mapping = scannedAsset.LocationMappings.FirstOrDefault(m => m.IsCurrent);
        if (mapping is null)
        {
            StatusMessage = "Asset has no assigned location to verify against.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        var asset = await _unitOfWork.Assets.GetByIdAsync(scannedAsset.Id);
        if (asset is null) return;

        asset.IsVerified = true;
        asset.LastVerifiedDate = DateTime.UtcNow;
        _unitOfWork.Assets.Update(asset);

        await _unitOfWork.VerificationLogs.AddAsync(new VerificationLog
        {
            AssetId = asset.Id,
            VerifiedByUserId = _authService.CurrentUser!.Id,
            VerifiedDate = DateTime.UtcNow,
            LocationId = mapping.LocationId,
            Remarks = Remarks
        });

        await _unitOfWork.SaveChangesAsync();
        StatusMessage = $"{asset.AssetName} verified successfully.";
        _toastService.Show(StatusMessage, ToastKind.Success);
        ResetScan();
    }

    private async Task ConfirmLocationVerificationAsync(Location scannedLocation)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(scannedLocation.Id);
        if (location is null) return;

        location.IsVerified = true;
        location.LastVerifiedDate = DateTime.UtcNow;
        _unitOfWork.Locations.Update(location);

        await _unitOfWork.LocationVerificationLogs.AddAsync(new LocationVerificationLog
        {
            LocationId = location.Id,
            VerifiedByUserId = _authService.CurrentUser!.Id,
            VerifiedDate = DateTime.UtcNow,
            Remarks = Remarks
        });

        await _unitOfWork.SaveChangesAsync();
        StatusMessage = $"{location.LocationName} verified successfully.";
        _toastService.Show(StatusMessage, ToastKind.Success);
        ResetScan();
    }

    [RelayCommand]
    private void ResetScan()
    {
        ScannedAsset = null;
        ScannedLocation = null;
        ScannedLocationChain = string.Empty;
        Remarks = string.Empty;
        IsScanning = true;
        OnPropertyChanged(nameof(HasResult));
    }
}
