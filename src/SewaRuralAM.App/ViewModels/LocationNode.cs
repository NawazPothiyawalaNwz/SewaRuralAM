using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

public partial class LocationNode : ObservableObject
{
    public Location Location { get; }

    [ObservableProperty]
    private bool isExpanded;

    public ObservableCollection<LocationNode> Children { get; } = new();

    public bool CanAddChild => Location.LevelNo < Location.MaxLevel;

    public LocationNode(Location location)
    {
        Location = location;
    }
}
