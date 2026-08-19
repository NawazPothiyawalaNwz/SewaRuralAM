using CommunityToolkit.Mvvm.ComponentModel;
using SewaRuralAM.Core.Entities;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

public partial class AssetSelectionRow : ObservableObject
{
    public Asset Asset { get; }
    public string LocationChain { get; }

    [ObservableProperty]
    private bool isSelected;

    public AssetSelectionRow(Asset asset, string locationChain)
    {
        Asset = asset;
        LocationChain = locationChain;
    }
}

public partial class LocationSelectionRow : ObservableObject
{
    public Location Location { get; }
    public string Chain { get; }

    [ObservableProperty]
    private bool isSelected;

    public LocationSelectionRow(Location location, string chain)
    {
        Location = location;
        Chain = chain;
    }
}
