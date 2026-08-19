using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SewaRuralAM.App.Services;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class AssetQrPrintViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQrCodeService _qrCodeService;
    private readonly IPdfService _pdfService;
    private readonly IToastService _toastService;
    private List<AssetSelectionRow> _allRows = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private LocationOption? selectedLocationFilter;

    [ObservableProperty]
    private int selectedCount;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ObservableCollection<AssetSelectionRow> Rows { get; } = new();
    public ObservableCollection<LocationOption> LocationFilterOptions { get; } = new();

    public AssetQrPrintViewModel(IUnitOfWork unitOfWork, IQrCodeService qrCodeService, IPdfService pdfService, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _qrCodeService = qrCodeService;
        _pdfService = pdfService;
        _toastService = toastService;
        Title = "Print Asset QR Codes";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            LocationFilterOptions.Clear();
            foreach (var option in LocationOption.BuildLevelSixOptions(allLocations))
                LocationFilterOptions.Add(option);

            var assets = await _unitOfWork.Assets.Query().Include(a => a.AssetCategory).ToListAsync();
            var currentMappings = await _unitOfWork.AssetLocationMappings.FindAsync(m => m.IsCurrent);
            var byId = LocationChainHelper.ToLookup(allLocations);

            _allRows = assets
                .OrderBy(a => a.AssetName)
                .Select(asset =>
                {
                    var mapping = currentMappings.FirstOrDefault(m => m.AssetId == asset.Id);
                    var chain = mapping is not null && byId.TryGetValue(mapping.LocationId, out var loc)
                        ? LocationChainHelper.BuildChain(loc, byId)
                        : "Not assigned";
                    return new AssetSelectionRow(asset, chain);
                })
                .ToList();

            foreach (var row in _allRows)
                row.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(AssetSelectionRow.IsSelected))
                        UpdateSelectedCount();
                };

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedLocationFilterChanged(LocationOption? value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<AssetSelectionRow> query = _allRows;

        if (SelectedLocationFilter is not null)
            query = query.Where(r => r.LocationChain == SelectedLocationFilter.Chain);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var text = SearchText.Trim();
            query = query.Where(r =>
                r.Asset.AssetName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                r.Asset.AssetCode.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        Rows.Clear();
        foreach (var row in query)
            Rows.Add(row);

        UpdateSelectedCount();
    }

    private void UpdateSelectedCount() => SelectedCount = _allRows.Count(r => r.IsSelected);

    [RelayCommand]
    private void SelectAllVisible()
    {
        foreach (var row in Rows)
            row.IsSelected = true;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var row in _allRows)
            row.IsSelected = false;
    }

    [RelayCommand]
    private void ClearLocationFilter() => SelectedLocationFilter = null;

    [RelayCommand]
    private async Task PrintSelectedAsync()
    {
        var selected = _allRows.Where(r => r.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Select at least one asset to print.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        var items = selected.Select(row => (
            row.Asset.AssetCode,
            row.Asset.AssetName,
            _qrCodeService.GenerateQrCode($"ASSET|{row.Asset.Id}|{row.Asset.AssetCode}")));

        var pdfBytes = _pdfService.GenerateAssetQrSheet(items);
        var filePath = await PdfFileHelper.SaveAndOpenAsync(pdfBytes, "AssetQrCodes");

        StatusMessage = $"Saved {selected.Count} QR code(s) to {filePath}";
        _toastService.Show($"{selected.Count} asset QR code(s) exported to PDF.", ToastKind.Success);
    }
}
