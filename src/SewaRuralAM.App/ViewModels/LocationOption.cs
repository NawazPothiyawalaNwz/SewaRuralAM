using SewaRuralAM.Core.Services;
using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

// Represents a Level-6 (the deepest allowed) location for asset-to-location assignment,
// displayed with its full ancestor chain, e.g. "Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A".
// Only fully built-out branches that reach Level 6 produce an option here by design — a branch
// that stops at, say, Level 3 will not appear until it is extended down to Level 6.
public class LocationOption
{
    public Location Location { get; }
    public string Chain { get; }

    public LocationOption(Location location, string chain)
    {
        Location = location;
        Chain = chain;
    }

    public static List<LocationOption> BuildLevelSixOptions(IEnumerable<Location> allLocations)
    {
        var locations = allLocations.ToList();
        var byId = LocationChainHelper.ToLookup(locations);

        var levelSix = locations.Where(l => l.LevelNo == Location.MaxLevel);

        return levelSix
            .Select(location => new LocationOption(location, LocationChainHelper.BuildChain(location, byId)))
            .OrderBy(o => o.Chain)
            .ToList();
    }
}
