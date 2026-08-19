using SewaRuralAM.Core.Entities;

namespace SewaRuralAM.Core.Services;

// Pure, dependency-free location-hierarchy logic (no MAUI/EF dependency) so it's directly
// unit-testable and shared by every ViewModel that needs to render a full breadcrumb chain,
// e.g. "Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A".
public static class LocationChainHelper
{
    public static Dictionary<int, Location> ToLookup(IEnumerable<Location> allLocations) =>
        allLocations.ToDictionary(l => l.Id);

    public static string BuildChain(Location location, Dictionary<int, Location> byId)
    {
        var names = new List<string>();
        Location? current = location;

        while (current is not null)
        {
            names.Insert(0, current.LocationName);
            current = current.ParentLocationId.HasValue && byId.TryGetValue(current.ParentLocationId.Value, out var parent)
                ? parent
                : null;
        }

        return string.Join(" > ", names);
    }
}
