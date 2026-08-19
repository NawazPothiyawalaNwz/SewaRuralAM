using Location = SewaRuralAM.Core.Entities.Location;

namespace SewaRuralAM.App.ViewModels;

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
