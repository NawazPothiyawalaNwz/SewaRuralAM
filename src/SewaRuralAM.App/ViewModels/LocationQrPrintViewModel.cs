using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.App.Services;
using SewaRuralAM.Core.Interfaces;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

public partial class LocationQrPrintViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQrCodeService _qrCodeService;
    private readonly IPdfService _pdfService;
    private readonly IToastService _toastService;
    private List<LocationSelectionRow> _allRows = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private int selectedCount;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ObservableCollection<LocationSelectionRow> Rows { get; } = new();

    public LocationQrPrintViewModel(IUnitOfWork unitOfWork, IQrCodeService qrCodeService, IPdfService pdfService, IToastService toastService)
    {
        _unitOfWork = unitOfWork;
        _qrCodeService = qrCodeService;
        _pdfService = pdfService;
        _toastService = toastService;
        Title = "Print Location QR Codes";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var byId = LocationChainHelper.ToLookup(allLocations);

            // Only Level 6 locations are "complete" locations in practice — printing labels for
            // intermediate levels (Building/Floor/etc.) isn't meaningful for this deployment.
            _allRows = allLocations
                .Where(l => l.LevelNo == Location.MaxLevel)
                .OrderBy(l => l.LocationName)
                .Select(l => new LocationSelectionRow(l, LocationChainHelper.BuildChain(l, byId)))
                .ToList();

            foreach (var row in _allRows)
                row.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(LocationSelectionRow.IsSelected))
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

    private void ApplyFilter()
    {
        IEnumerable<LocationSelectionRow> query = _allRows;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var text = SearchText.Trim();
            query = query.Where(r =>
                r.Location.LocationName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                r.Location.LocationCode.Contains(text, StringComparison.OrdinalIgnoreCase));
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
    private async Task PrintSelectedAsync()
    {
        var selected = _allRows.Where(r => r.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Select at least one location to print.";
            _toastService.Show(StatusMessage, ToastKind.Error);
            return;
        }

        var items = selected.Select(row => (
            row.Location.LocationCode,
            row.Location.LocationName,
            _qrCodeService.GenerateQrCode($"LOCATION|{row.Location.Id}|{row.Location.LocationCode}")));

        var pdfBytes = _pdfService.GenerateAssetQrSheet(items);
        var filePath = await PdfFileHelper.SaveAndOpenAsync(pdfBytes, "LocationQrCodes");

        StatusMessage = $"Saved {selected.Count} QR code(s) to {filePath}";
        _toastService.Show($"{selected.Count} location QR code(s) exported to PDF.", ToastKind.Success);
    }
}
