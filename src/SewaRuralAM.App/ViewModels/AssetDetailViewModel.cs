using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Enums;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

[QueryProperty(nameof(AssetId), "assetId")]
public partial class AssetDetailViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQrCodeService _qrCodeService;
    private readonly IAuthService _authService;
    private readonly IToastService _toastService;
    private readonly IMenuAccessService _menuAccessService;

    [ObservableProperty]
    private int assetId;

    [ObservableProperty]
    private string assetCode = string.Empty;

    [ObservableProperty]
    private string assetName = string.Empty;

    [ObservableProperty]
    private string? brand;

    [ObservableProperty]
    private string? model;

    [ObservableProperty]
    private string? serialNumber;

    [ObservableProperty]
    private string? vendor;

    [ObservableProperty]
    private decimal? purchaseCost;

    [ObservableProperty]
    private DateTime purchaseDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? warrantyExpiry;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private AssetStatus status = AssetStatus.Active;

    [ObservableProperty]
    private AssetCategory? selectedCategory;

    [ObservableProperty]
    private LocationOption? selectedLocationOption;

    [ObservableProperty]
    private byte[]? qrImage;

    [ObservableProperty]
    private bool isVerified;

    [ObservableProperty]
    private DateTime? lastVerifiedDate;

    [ObservableProperty]
    private string lastVerifiedByName = string.Empty;

    [ObservableProperty]
    private string currentLocationName = string.Empty;

    [ObservableProperty]
    private bool hasNoLevelSixLocations;

    [ObservableProperty]
    private bool canSave;

    [ObservableProperty]
    private bool canEdit;

    [ObservableProperty]
    private bool canDelete;

    public ObservableCollection<AssetCategory> Categories { get; } = new();
    public ObservableCollection<LocationOption> LocationOptions { get; } = new();
    public ObservableCollection<VerificationLog> VerificationHistory { get; } = new();
    public IReadOnlyList<AssetStatus> StatusOptions { get; } = Enum.GetValues<AssetStatus>();

    public bool IsNewAsset => AssetId == 0;

    public AssetDetailViewModel(IUnitOfWork unitOfWork, IQrCodeService qrCodeService, IAuthService authService, IToastService toastService, IMenuAccessService menuAccessService)
    {
        _unitOfWork = unitOfWork;
        _qrCodeService = qrCodeService;
        _authService = authService;
        _toastService = toastService;
        _menuAccessService = menuAccessService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var rights = await _menuAccessService.GetRightsAsync("AssetListPage");
            CanSave = IsNewAsset ? rights.CanAdd : rights.CanEdit;
            CanEdit = rights.CanEdit;
            CanDelete = rights.CanDelete;

            Categories.Clear();
            foreach (var category in await _unitOfWork.AssetCategories.GetAllAsync())
                Categories.Add(category);

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            LocationOptions.Clear();
            foreach (var option in LocationOption.BuildLevelSixOptions(allLocations))
                LocationOptions.Add(option);
            HasNoLevelSixLocations = LocationOptions.Count == 0;

            if (AssetId > 0)
            {
                var asset = await _unitOfWork.Assets.Query()
                    .Include(a => a.AssetCategory)
                    .Include(a => a.LocationMappings).ThenInclude(m => m.Location)
                    .Include(a => a.VerificationLogs).ThenInclude(v => v.VerifiedByUser)
                    .FirstOrDefaultAsync(a => a.Id == AssetId);

                if (asset is null) return;

                Title = asset.AssetName;
                AssetCode = asset.AssetCode;
                AssetName = asset.AssetName;
                Brand = asset.Brand;
                Model = asset.Model;
                SerialNumber = asset.SerialNumber;
                Vendor = asset.Vendor;
                PurchaseCost = asset.PurchaseCost;
                PurchaseDate = asset.PurchaseDate ?? DateTime.Today;
                WarrantyExpiry = asset.WarrantyExpiry;
                Description = asset.Description;
                Status = asset.Status;
                IsVerified = asset.IsVerified;
                LastVerifiedDate = asset.LastVerifiedDate;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == asset.AssetCategoryId);

                var currentMapping = asset.LocationMappings.FirstOrDefault(m => m.IsCurrent);
                SelectedLocationOption = currentMapping is null
                    ? null
                    : LocationOptions.FirstOrDefault(l => l.Location.Id == currentMapping.LocationId);
                CurrentLocationName = SelectedLocationOption?.Chain ?? currentMapping?.Location.LocationName ?? "Not assigned";

                VerificationHistory.Clear();
                foreach (var log in asset.VerificationLogs.OrderByDescending(v => v.VerifiedDate))
                    VerificationHistory.Add(log);

                LastVerifiedByName = VerificationHistory.FirstOrDefault()?.VerifiedByUser?.FullName ?? string.Empty;

                QrImage = _qrCodeService.GenerateQrCode($"ASSET|{asset.Id}|{asset.AssetCode}");
            }
            else
            {
                Title = "New Asset";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!CanSave)
        {
            _toastService.Show("You don't have permission to save this asset.", ToastKind.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(AssetCode) || string.IsNullOrWhiteSpace(AssetName) || SelectedCategory is null)
        {
            _toastService.Show("Asset code, name and category are required.", ToastKind.Error);
            return;
        }

        Asset asset;
        var isNew = AssetId == 0;

        if (isNew)
        {
            asset = new Asset();
        }
        else
        {
            asset = await _unitOfWork.Assets.GetByIdAsync(AssetId) ?? new Asset();
        }

        asset.AssetCode = AssetCode.Trim();
        asset.AssetName = AssetName.Trim();
        asset.AssetCategoryId = SelectedCategory.Id;
        asset.Brand = Brand;
        asset.Model = Model;
        asset.SerialNumber = SerialNumber;
        asset.Vendor = Vendor;
        asset.PurchaseCost = PurchaseCost;
        asset.PurchaseDate = PurchaseDate;
        asset.WarrantyExpiry = WarrantyExpiry;
        asset.Description = Description;
        asset.Status = Status;

        if (isNew)
        {
            await _unitOfWork.Assets.AddAsync(asset);
        }
        else
        {
            _unitOfWork.Assets.Update(asset);
        }

        await _unitOfWork.SaveChangesAsync();

        if (SelectedLocationOption is not null)
        {
            var locationId = SelectedLocationOption.Location.Id;
            var currentMapping = await _unitOfWork.AssetLocationMappings
                .FirstOrDefaultAsync(m => m.AssetId == asset.Id && m.IsCurrent);

            if (currentMapping is null || currentMapping.LocationId != locationId)
            {
                if (currentMapping is not null)
                {
                    currentMapping.IsCurrent = false;
                    currentMapping.UnassignedDate = DateTime.UtcNow;
                    _unitOfWork.AssetLocationMappings.Update(currentMapping);
                }

                await _unitOfWork.AssetLocationMappings.AddAsync(new AssetLocationMapping
                {
                    AssetId = asset.Id,
                    LocationId = locationId,
                    AssignedDate = DateTime.UtcNow,
                    IsCurrent = true
                });

                await _unitOfWork.SaveChangesAsync();
            }
        }

        _toastService.Show(isNew ? $"Asset '{asset.AssetName}' created." : $"Asset '{asset.AssetName}' updated.", ToastKind.Success);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (AssetId == 0 || _authService.CurrentUser is null)
            return;

        if (!CanEdit)
        {
            _toastService.Show("You don't have permission to verify assets.", ToastKind.Error);
            return;
        }

        if (SelectedLocationOption is null)
        {
            _toastService.Show("Assign a location before verifying this asset.", ToastKind.Error);
            return;
        }

        var asset = await _unitOfWork.Assets.GetByIdAsync(AssetId);
        if (asset is null) return;

        asset.IsVerified = true;
        asset.LastVerifiedDate = DateTime.UtcNow;
        _unitOfWork.Assets.Update(asset);

        await _unitOfWork.VerificationLogs.AddAsync(new VerificationLog
        {
            AssetId = asset.Id,
            VerifiedByUserId = _authService.CurrentUser.Id,
            VerifiedDate = DateTime.UtcNow,
            LocationId = SelectedLocationOption.Location.Id
        });

        await _unitOfWork.SaveChangesAsync();
        _toastService.Show($"'{asset.AssetName}' marked as verified.", ToastKind.Success);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (AssetId == 0) return;

        if (!CanDelete)
        {
            _toastService.Show("You don't have permission to delete assets.", ToastKind.Error);
            return;
        }

        var asset = await _unitOfWork.Assets.GetByIdAsync(AssetId);
        if (asset is null) return;

        _unitOfWork.Assets.Remove(asset);
        await _unitOfWork.SaveChangesAsync();
        _toastService.Show($"'{asset.AssetName}' deleted.", ToastKind.Success);
        await Shell.Current.GoToAsync("..");
    }
}
