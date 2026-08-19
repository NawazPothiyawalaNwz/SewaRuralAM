using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

[QueryProperty(nameof(Filter), "filter")]
public partial class AssetListViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private List<Asset> _allAssets = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string filter = string.Empty;

    public ObservableCollection<Asset> Assets { get; } = new();

    public AssetListViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        Title = "Assets";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            _allAssets = await _unitOfWork.Assets.Query()
                .Include(a => a.AssetCategory)
                .ToListAsync();
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<Asset> query = _allAssets;

        if (Filter == "verified")
            query = query.Where(a => a.IsVerified);
        else if (Filter == "pending")
            query = query.Where(a => !a.IsVerified);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var text = SearchText.Trim();
            query = query.Where(a =>
                a.AssetName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                a.AssetCode.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        Assets.Clear();
        foreach (var asset in query)
            Assets.Add(asset);
    }

    [RelayCommand]
    private static async Task OpenAssetAsync(Asset asset)
    {
        if (asset is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.AssetDetailPage)}?assetId={asset.Id}");
    }

    [RelayCommand]
    private static async Task AddAssetAsync() =>
        await Shell.Current.GoToAsync($"{nameof(Views.AssetDetailPage)}?assetId=0");

    [RelayCommand]
    private static async Task OpenQrPrintAsync() =>
        await Shell.Current.GoToAsync(nameof(Views.AssetQrPrintPage));
}
