using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Interfaces;
using SewaRuralAM.Core.Services;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    private int totalAssets;

    [ObservableProperty]
    private int totalLocations;

    [ObservableProperty]
    private int verifiedAssets;

    [ObservableProperty]
    private int pendingVerification;

    public ObservableCollection<ChartItem> AssetsByLocation { get; } = new();
    public ObservableCollection<ChartItem> AssetsByCategory { get; } = new();

    public DashboardViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var assets = await _unitOfWork.Assets.GetAllAsync();
            var locations = await _unitOfWork.Locations.GetAllAsync();

            TotalAssets = assets.Count;
            TotalLocations = locations.Count(l => l.LevelNo == Location.MaxLevel);
            VerifiedAssets = assets.Count(a => a.IsVerified);
            PendingVerification = assets.Count(a => !a.IsVerified);

            var categories = await _unitOfWork.AssetCategories.GetAllAsync();
            var byCategory = assets
                .GroupBy(a => a.AssetCategoryId)
                .Select(g => new ChartItem
                {
                    Label = categories.FirstOrDefault(c => c.Id == g.Key)?.CategoryName ?? "Unknown",
                    Value = g.Count()
                })
                .OrderByDescending(c => c.Value)
                .ToList();

            var maxCategory = Math.Max(1, byCategory.Select(c => c.Value).DefaultIfEmpty(0).Max());
            AssetsByCategory.Clear();
            foreach (var item in byCategory)
            {
                item.Percent = (double)item.Value / maxCategory;
                AssetsByCategory.Add(item);
            }

            var mappings = await _unitOfWork.AssetLocationMappings.FindAsync(m => m.IsCurrent);
            var byId = LocationChainHelper.ToLookup(locations);
            var byLocation = mappings
                .GroupBy(m => m.LocationId)
                .Select(g => new ChartItem
                {
                    Label = byId.TryGetValue(g.Key, out var loc) ? LocationChainHelper.BuildChain(loc, byId) : "Unknown",
                    Value = g.Count()
                })
                .OrderByDescending(c => c.Value)
                .Take(10)
                .ToList();

            var maxLocation = Math.Max(1, byLocation.Select(c => c.Value).DefaultIfEmpty(0).Max());
            AssetsByLocation.Clear();
            foreach (var item in byLocation)
            {
                item.Percent = (double)item.Value / maxLocation;
                AssetsByLocation.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task OpenAssetsAsync() =>
        await Shell.Current.GoToAsync($"//{nameof(Views.AssetListPage)}?filter=all");

    [RelayCommand]
    private static async Task OpenLocationsAsync() =>
        await Shell.Current.GoToAsync($"//{nameof(Views.LocationTreePage)}");

    [RelayCommand]
    private static async Task OpenVerifiedAssetsAsync() =>
        await Shell.Current.GoToAsync($"//{nameof(Views.AssetListPage)}?filter=verified");

    [RelayCommand]
    private static async Task OpenPendingAssetsAsync() =>
        await Shell.Current.GoToAsync($"//{nameof(Views.AssetListPage)}?filter=pending");
}
