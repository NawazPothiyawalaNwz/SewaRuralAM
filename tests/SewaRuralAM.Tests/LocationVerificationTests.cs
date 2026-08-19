using SewaRuralAM.Core.Entities;
using SewaRuralAM.Infrastructure.Repositories;
using Xunit;

namespace SewaRuralAM.Tests;

public class LocationVerificationTests
{
    [Fact]
    public async Task VerifyingLocation_SetsIsVerifiedAndCreatesLog()
    {
        using var factory = new TestDbContextFactory();
        using var unitOfWork = new UnitOfWork(factory);

        var user = new User { UserName = "admin", FullName = "Admin", PasswordHash = "x" };
        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        var location = new Location { LocationCode = "LOC-A", LocationName = "Shelf A", LevelNo = 6 };
        await unitOfWork.Locations.AddAsync(location);
        await unitOfWork.SaveChangesAsync();

        Assert.False(location.IsVerified);

        location.IsVerified = true;
        location.LastVerifiedDate = DateTime.UtcNow;
        unitOfWork.Locations.Update(location);

        await unitOfWork.LocationVerificationLogs.AddAsync(new LocationVerificationLog
        {
            LocationId = location.Id,
            VerifiedByUserId = user.Id,
            VerifiedDate = DateTime.UtcNow,
            Remarks = "Checked physically"
        });
        await unitOfWork.SaveChangesAsync();

        var reloaded = await unitOfWork.Locations.GetByIdAsync(location.Id);
        var logs = await unitOfWork.LocationVerificationLogs.FindAsync(l => l.LocationId == location.Id);

        Assert.True(reloaded!.IsVerified);
        Assert.NotNull(reloaded.LastVerifiedDate);
        Assert.Single(logs);
        Assert.Equal(user.Id, logs[0].VerifiedByUserId);
    }
}
