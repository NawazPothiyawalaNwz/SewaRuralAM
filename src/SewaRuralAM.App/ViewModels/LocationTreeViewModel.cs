using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Interfaces;

namespace SewaRuralAM.App.ViewModels;

public partial class LocationTreeViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private List<LocationNode> _roots = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    public ObservableCollection<LocationNode> VisibleNodes { get; } = new();

    public LocationTreeViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        Title = "Locations";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var locations = await _unitOfWork.Locations.GetAllAsync();
            var nodesById = locations.ToDictionary(l => l.Id, l => new LocationNode(l));

            _roots = new List<LocationNode>();
            foreach (var location in locations.OrderBy(l => l.LevelNo).ThenBy(l => l.LocationName))
            {
                var node = nodesById[location.Id];
                if (location.ParentLocationId.HasValue && nodesById.TryGetValue(location.ParentLocationId.Value, out var parent))
                    parent.Children.Add(node);
                else
                    _roots.Add(node);
            }

            foreach (var root in _roots)
                root.IsExpanded = true;

            Rebuild();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => Rebuild();

    [RelayCommand]
    private void ToggleExpand(LocationNode node)
    {
        node.IsExpanded = !node.IsExpanded;
        Rebuild();
    }

    private void Rebuild()
    {
        VisibleNodes.Clear();
        var hasSearch = !string.IsNullOrWhiteSpace(SearchText);

        foreach (var root in _roots)
            AddVisible(root, hasSearch);
    }

    private void AddVisible(LocationNode node, bool hasSearch)
    {
        var matches = !hasSearch ||
            node.Location.LocationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            node.Location.LocationCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

        var childMatches = node.Children.Any(c => NodeOrDescendantMatches(c));

        if (matches || childMatches || !hasSearch)
        {
            VisibleNodes.Add(node);

            if (node.IsExpanded || (hasSearch && childMatches))
            {
                foreach (var child in node.Children)
                    AddVisible(child, hasSearch);
            }
        }
    }

    private bool NodeOrDescendantMatches(LocationNode node)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        if (node.Location.LocationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            node.Location.LocationCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            return true;

        return node.Children.Any(NodeOrDescendantMatches);
    }

    [RelayCommand]
    private static async Task AddLocationAsync() =>
        await Shell.Current.GoToAsync($"{nameof(Views.LocationEditPage)}?locationId=0");

    [RelayCommand]
    private static async Task AddChildLocationAsync(LocationNode node)
    {
        if (node is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.LocationEditPage)}?locationId=0&parentId={node.Location.Id}");
    }

    [RelayCommand]
    private static async Task EditLocationAsync(LocationNode node)
    {
        if (node is null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.LocationEditPage)}?locationId={node.Location.Id}");
    }

    [RelayCommand]
    private static async Task OpenQrPrintAsync() =>
        await Shell.Current.GoToAsync(nameof(Views.LocationQrPrintPage));
}
