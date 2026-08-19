using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Services;
using Xunit;

namespace SewaRuralAM.Tests;

public class LocationChainHelperTests
{
    private static List<Location> BuildSixLevelBranch()
    {
        var headOffice = new Location { Id = 1, LocationName = "Head Office", LevelNo = 1 };
        var buildingA = new Location { Id = 2, LocationName = "Building A", LevelNo = 2, ParentLocationId = 1 };
        var floor1 = new Location { Id = 3, LocationName = "Floor 1", LevelNo = 3, ParentLocationId = 2 };
        var room101 = new Location { Id = 4, LocationName = "Room 101", LevelNo = 4, ParentLocationId = 3 };
        var rack1 = new Location { Id = 5, LocationName = "Rack 1", LevelNo = 5, ParentLocationId = 4 };
        var shelfA = new Location { Id = 6, LocationName = "Shelf A", LevelNo = 6, ParentLocationId = 5 };

        return new List<Location> { headOffice, buildingA, floor1, room101, rack1, shelfA };
    }

    [Fact]
    public void BuildChain_ForLeafLocation_JoinsFullAncestorChain()
    {
        var locations = BuildSixLevelBranch();
        var byId = LocationChainHelper.ToLookup(locations);
        var shelfA = locations.Single(l => l.Id == 6);

        var chain = LocationChainHelper.BuildChain(shelfA, byId);

        Assert.Equal("Head Office > Building A > Floor 1 > Room 101 > Rack 1 > Shelf A", chain);
    }

    [Fact]
    public void BuildChain_ForRootLocation_ReturnsJustItsOwnName()
    {
        var locations = BuildSixLevelBranch();
        var byId = LocationChainHelper.ToLookup(locations);
        var headOffice = locations.Single(l => l.Id == 1);

        var chain = LocationChainHelper.BuildChain(headOffice, byId);

        Assert.Equal("Head Office", chain);
    }

    [Fact]
    public void BuildChain_ForMidLevelLocation_StopsAtThatLocation()
    {
        var locations = BuildSixLevelBranch();
        var byId = LocationChainHelper.ToLookup(locations);
        var room101 = locations.Single(l => l.Id == 4);

        var chain = LocationChainHelper.BuildChain(room101, byId);

        Assert.Equal("Head Office > Building A > Floor 1 > Room 101", chain);
    }

    [Fact]
    public void ToLookup_KeysLocationsById()
    {
        var locations = BuildSixLevelBranch();

        var byId = LocationChainHelper.ToLookup(locations);

        Assert.Equal(locations.Count, byId.Count);
        Assert.Equal("Rack 1", byId[5].LocationName);
    }
}
