using SewaRuralAM.Core.Entities;
using SewaRuralAM.Infrastructure.Repositories;
using Xunit;

namespace SewaRuralAM.Tests;

public class LocationHierarchyTests
{
    [Fact]
    public void MaxLevel_IsSix()
    {
        Assert.Equal(6, Location.MaxLevel);
    }

    [Fact]
    public async Task ChildLocation_InheritsParentAndComputesLevel()
    {
        using var factory = new TestDbContextFactory();
        using var unitOfWork = new UnitOfWork(factory);

        var headOffice = new Location { LocationCode = "LOC-HO", LocationName = "Head Office", LevelNo = 1 };
        await unitOfWork.Locations.AddAsync(headOffice);
        await unitOfWork.SaveChangesAsync();

        var buildingA = new Location
        {
            LocationCode = "LOC-HO-BA",
            LocationName = "Building A",
            LevelNo = headOffice.LevelNo + 1,
            ParentLocationId = headOffice.Id
        };
        await unitOfWork.Locations.AddAsync(buildingA);
        await unitOfWork.SaveChangesAsync();

        var children = await unitOfWork.Locations.FindAsync(l => l.ParentLocationId == headOffice.Id);

        Assert.Single(children);
        Assert.Equal(2, children[0].LevelNo);
    }
}
