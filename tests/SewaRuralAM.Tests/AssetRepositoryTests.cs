using SewaRuralAM.Core.Entities;
using SewaRuralAM.Core.Enums;
using SewaRuralAM.Infrastructure.Repositories;
using Xunit;

namespace SewaRuralAM.Tests;

public class AssetRepositoryTests
{
    [Fact]
    public async Task AddAsync_Then_GetByIdAsync_ReturnsSameAsset()
    {
        using var factory = new TestDbContextFactory();
        using var unitOfWork = new UnitOfWork(factory);

        var category = new AssetCategory { CategoryName = "Computers" };
        await unitOfWork.AssetCategories.AddAsync(category);
        await unitOfWork.SaveChangesAsync();

        var asset = new Asset
        {
            AssetCode = "AST-0001",
            AssetName = "Test Laptop",
            AssetCategoryId = category.Id,
            Status = AssetStatus.Active
        };
        await unitOfWork.Assets.AddAsync(asset);
        await unitOfWork.SaveChangesAsync();

        var loaded = await unitOfWork.Assets.GetByIdAsync(asset.Id);

        Assert.NotNull(loaded);
        Assert.Equal("AST-0001", loaded!.AssetCode);
        Assert.False(loaded.IsVerified);
    }

    [Fact]
    public async Task Update_PersistsChangesAndSetsModifiedDate()
    {
        using var factory = new TestDbContextFactory();
        using var unitOfWork = new UnitOfWork(factory);

        var category = new AssetCategory { CategoryName = "Furniture" };
        await unitOfWork.AssetCategories.AddAsync(category);
        await unitOfWork.SaveChangesAsync();

        var asset = new Asset { AssetCode = "AST-0002", AssetName = "Chair", AssetCategoryId = category.Id };
        await unitOfWork.Assets.AddAsync(asset);
        await unitOfWork.SaveChangesAsync();

        asset.AssetName = "Office Chair";
        unitOfWork.Assets.Update(asset);
        await unitOfWork.SaveChangesAsync();

        var reloaded = await unitOfWork.Assets.GetByIdAsync(asset.Id);

        Assert.Equal("Office Chair", reloaded!.AssetName);
        Assert.NotNull(reloaded.ModifiedDate);
    }

    [Fact]
    public async Task Remove_DeletesAsset()
    {
        using var factory = new TestDbContextFactory();
        using var unitOfWork = new UnitOfWork(factory);

        var category = new AssetCategory { CategoryName = "Vehicles" };
        await unitOfWork.AssetCategories.AddAsync(category);
        await unitOfWork.SaveChangesAsync();

        var asset = new Asset { AssetCode = "AST-0003", AssetName = "Van", AssetCategoryId = category.Id };
        await unitOfWork.Assets.AddAsync(asset);
        await unitOfWork.SaveChangesAsync();

        unitOfWork.Assets.Remove(asset);
        await unitOfWork.SaveChangesAsync();

        var reloaded = await unitOfWork.Assets.GetByIdAsync(asset.Id);
        Assert.Null(reloaded);
    }
}
