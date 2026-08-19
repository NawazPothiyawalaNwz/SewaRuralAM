using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

[QueryProperty(nameof(LocationId), "locationId")]
[QueryProperty(nameof(PresetParentId), "parentId")]
public partial class LocationEditViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQrCodeService _qrCodeService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private int locationId;

    [ObservableProperty]
    private int presetParentId;

    [ObservableProperty]
    private string locationCode = string.Empty;

    [ObservableProperty]
    private string locationName = string.Empty;

    [ObservableProperty]
    private Location? selectedParent;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private byte[]? qrImage;

    [ObservableProperty]
    private string levelPreview = string.Empty;

    [ObservableProperty]
    private bool isVerified;

    [ObservableProperty]
    private DateTime? lastVerifiedDate;

    [ObservableProperty]
    private string lastVerifiedByName = string.Empty;

    public ObservableCollection<Location> ParentOptions { get; } = new();
    public ObservableCollection<LocationVerificationLog> VerificationHistory { get; } = new();

    public bool IsNewLocation => LocationId == 0;

    public LocationEditViewModel(IUnitOfWork unitOfWork, IQrCodeService qrCodeService, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _qrCodeService = qrCodeService;
        _toastService = toastService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var allLocations = await _unitOfWork.Locations.FindAsync(l => l.LevelNo < Location.MaxLevel);

        ParentOptions.Clear();
        foreach (var location in allLocations.Where(l => l.Id != LocationId))
            ParentOptions.Add(location);

        if (LocationId > 0)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(LocationId);
            if (location is null) return;

            Title = "Edit Location";
            LocationCode = location.LocationCode;
            LocationName = location.LocationName;
            SelectedParent = location.ParentLocationId.HasValue
                ? ParentOptions.FirstOrDefault(p => p.Id == location.ParentLocationId.Value)
                : null;
            QrImage = _qrCodeService.GenerateQrCode($"LOCATION|{location.Id}|{location.LocationCode}");
            IsVerified = location.IsVerified;
            LastVerifiedDate = location.LastVerifiedDate;

            var logs = await _unitOfWork.LocationVerificationLogs.Query()
                .Include(l => l.VerifiedByUser)
                .Where(l => l.LocationId == LocationId)
                .OrderByDescending(l => l.VerifiedDate)
                .ToListAsync();

            VerificationHistory.Clear();
            foreach (var log in logs)
                VerificationHistory.Add(log);

            LastVerifiedByName = logs.FirstOrDefault()?.VerifiedByUser?.FullName ?? string.Empty;
        }
        else
        {
            Title = "New Location";
            if (PresetParentId > 0)
                SelectedParent = ParentOptions.FirstOrDefault(p => p.Id == PresetParentId);
        }

        UpdateLevelPreview();
    }

    partial void OnSelectedParentChanged(Location? value) => UpdateLevelPreview();

    private void UpdateLevelPreview()
    {
        var level = (SelectedParent?.LevelNo ?? 0) + 1;
        LevelPreview = level > Location.MaxLevel
            ? $"Level {level} exceeds the maximum of {Location.MaxLevel}."
            : $"This location will be created at Level {level} of {Location.MaxLevel}.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(LocationCode) || string.IsNullOrWhiteSpace(LocationName))
        {
            ErrorMessage = "Location code and name are required.";
            _toastService.Show(ErrorMessage, ToastKind.Error);
            return;
        }

        var level = (SelectedParent?.LevelNo ?? 0) + 1;
        if (level > Location.MaxLevel)
        {
            ErrorMessage = $"Maximum location depth of {Location.MaxLevel} levels reached.";
            _toastService.Show(ErrorMessage, ToastKind.Error);
            return;
        }

        Location location;
        var isNew = LocationId == 0;

        location = isNew
            ? new Location()
            : await _unitOfWork.Locations.GetByIdAsync(LocationId) ?? new Location();

        location.LocationCode = LocationCode.Trim();
        location.LocationName = LocationName.Trim();
        location.ParentLocationId = SelectedParent?.Id;
        location.LevelNo = level;

        if (isNew)
            await _unitOfWork.Locations.AddAsync(location);
        else
            _unitOfWork.Locations.Update(location);

        await _unitOfWork.SaveChangesAsync();
        _toastService.Show(isNew ? $"Location '{location.LocationName}' created." : $"Location '{location.LocationName}' updated.", ToastKind.Success);
        await Shell.Current.GoToAsync("..");
    }
}
