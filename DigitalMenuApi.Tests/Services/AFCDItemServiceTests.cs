using DigitalMenuApi.Data;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Implementations;
using DigitalMenuApi.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class AFCDItemServiceTests
{
    private readonly Mock<ILogger<AFCDItemService>> _mockLogger;

    public AFCDItemServiceTests()
    {
        _mockLogger = new Mock<ILogger<AFCDItemService>>();
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return new UnitOfWork(context);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var request = new CreateAFCDItemRequest
        {
            Name = "Chicken Breast",
            Variant = "raw, skinless",
            Calories = 165,
            ProteinG = 31,
            CarbsG = 0,
            FatG = 3.6M
        };

        var result = await sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Chicken Breast");
        result.Data.Calories.Should().Be(165);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturn_WhenExists()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var createResult = await sut.CreateAsync(new CreateAFCDItemRequest
        {
            Name = "Rice", Calories = 130, ProteinG = 2.7M, CarbsG = 28, FatG = 0.3M
        });

        var result = await sut.GetByIdAsync(createResult.Data!.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Rice");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var result = await sut.GetByIdAsync(999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("AFCD item not found");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingItems()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        await sut.CreateAsync(new CreateAFCDItemRequest { Name = "Chicken Breast", Calories = 165, ProteinG = 31, CarbsG = 0, FatG = 3.6M });
        await sut.CreateAsync(new CreateAFCDItemRequest { Name = "Chicken Thigh", Calories = 209, ProteinG = 26, CarbsG = 0, FatG = 10.9M });
        await sut.CreateAsync(new CreateAFCDItemRequest { Name = "Beef Steak", Calories = 271, ProteinG = 26, CarbsG = 0, FatG = 18 });

        var result = await sut.SearchAsync("chicken");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnAll_WhenNoQuery()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        await sut.CreateAsync(new CreateAFCDItemRequest { Name = "Item A", Calories = 100, ProteinG = 10, CarbsG = 10, FatG = 5 });
        await sut.CreateAsync(new CreateAFCDItemRequest { Name = "Item B", Calories = 200, ProteinG = 20, CarbsG = 20, FatG = 10 });

        var result = await sut.SearchAsync(null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSucceed_WhenExists()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var createResult = await sut.CreateAsync(new CreateAFCDItemRequest
        {
            Name = "Rice", Calories = 130, ProteinG = 2.7M, CarbsG = 28, FatG = 0.3M
        });

        var result = await sut.UpdateAsync(createResult.Data!.Id, new UpdateAFCDItemRequest { Name = "Brown Rice", Calories = 111 });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Brown Rice");
        result.Data.Calories.Should().Be(111);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var result = await sut.UpdateAsync(999, new UpdateAFCDItemRequest { Name = "Nope" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("AFCD item not found");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenNotUsed()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var createResult = await sut.CreateAsync(new CreateAFCDItemRequest
        {
            Name = "Temp Item", Calories = 50, ProteinG = 1, CarbsG = 10, FatG = 0.5M
        });

        var result = await sut.DeleteAsync(createResult.Data!.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var result = await sut.DeleteAsync(999);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("AFCD item not found");
    }

    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenUsedInDishes()
    {
        using var uow = CreateUnitOfWork();
        var sut = new AFCDItemService(uow, _mockLogger.Object);

        var createResult = await sut.CreateAsync(new CreateAFCDItemRequest
        {
            Name = "Chicken", Calories = 165, ProteinG = 31, CarbsG = 0, FatG = 3.6M
        });

        // Link it to a dish
        await uow.Categories.AddAsync(new Category { Id = 1, RestaurantId = 1, Name = "Main", Type = "food" });
        await uow.Dishes.AddAsync(new Dish { Id = 1, CategoryId = 1, Name = "Chicken Bowl", Price = 15, Calories = 165, ProteinG = 31, CarbsG = 0, FatG = 3.6M });
        await uow.SaveChangesAsync();

        // Manually add a DishIngredient linking to this AFCD item
        var context = uow.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(uow) as ApplicationDbContext;
        context!.Set<DishIngredient>().Add(new DishIngredient { DishId = 1, AfcdItemId = createResult.Data!.Id, Amount = 200 });
        await context.SaveChangesAsync();

        var result = await sut.DeleteAsync(createResult.Data.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot delete AFCD item");
    }
}
