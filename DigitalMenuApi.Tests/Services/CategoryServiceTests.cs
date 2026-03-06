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

public class CategoryServiceTests
{
    private readonly Mock<ILogger<CategoryService>> _mockLogger;

    public CategoryServiceTests()
    {
        _mockLogger = new Mock<ILogger<CategoryService>>();
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return new UnitOfWork(context);
    }

    private async Task SeedRestaurantAsync(UnitOfWork uow, int ownerId = 10)
    {
        await uow.Users.AddAsync(new User
        {
            Id = ownerId, Email = $"owner{ownerId}@test.com", FirstName = "Owner", LastName = "Test",
            PasswordHash = "hash", RoleId = 2, IsActive = true
        });
        await uow.Restaurants.AddAsync(new Restaurant
        {
            Id = 1, UserId = ownerId, Name = "Test Restaurant", Slug = "test-restaurant",
            Address = "123 Test St", IsActive = true
        });
        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldSucceed_WhenOwnerCreates()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var request = new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" };
        var result = await sut.CreateCategoryAsync(10, "restaurant_admin", request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Mains");
        result.Data.Type.Should().Be("food");
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldFail_WhenRestaurantNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new CategoryService(uow, _mockLogger.Object);

        var request = new CreateCategoryRequest { RestaurantId = 999, Name = "Mains", Type = "food" };
        var result = await sut.CreateCategoryAsync(10, "restaurant_admin", request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Restaurant not found");
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldFail_WhenUserDoesNotOwnRestaurant()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow, ownerId: 10);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var request = new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" };
        var result = await sut.CreateCategoryAsync(99, "restaurant_admin", request);

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldSucceed_WhenSystemAdminCreates()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow, ownerId: 10);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var request = new CreateCategoryRequest { RestaurantId = 1, Name = "Drinks", Type = "drink" };
        var result = await sut.CreateCategoryAsync(99, "system_admin", request);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetCategoriesByRestaurantAsync_ShouldReturnCategories()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Drinks", Type = "drink" });

        var result = await sut.GetCategoriesByRestaurantAsync(1, 10, "restaurant_admin");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategoriesByRestaurantAsync_ShouldFail_WhenUserDoesNotOwnRestaurant()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var result = await sut.GetCategoriesByRestaurantAsync(1, 99, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        var result = await sut.GetCategoryByIdAsync(createResult.Data!.Id, 10, "restaurant_admin");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Mains");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new CategoryService(uow, _mockLogger.Object);

        var result = await sut.GetCategoryByIdAsync(999, 10, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Category not found");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldSucceed_WhenOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        var result = await sut.UpdateCategoryAsync(createResult.Data!.Id, 10, "restaurant_admin", new UpdateCategoryRequest { Name = "Starters" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Starters");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldFail_WhenNotOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        var result = await sut.UpdateCategoryAsync(createResult.Data!.Id, 99, "restaurant_admin", new UpdateCategoryRequest { Name = "Hacked" });

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldSucceed_WhenEmptyAndOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        var result = await sut.DeleteCategoryAsync(createResult.Data!.Id, 10, "restaurant_admin");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldFail_WhenCategoryHasDishes()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        // Seed a dish in this category
        await uow.Dishes.AddAsync(new Dish { Id = 1, CategoryId = createResult.Data!.Id, Name = "Pasta", Price = 10.0M, Calories = 500, ProteinG = 20, CarbsG = 60, FatG = 15 });
        await uow.SaveChangesAsync();

        var result = await sut.DeleteCategoryAsync(createResult.Data.Id, 10, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot delete category with dishes");
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldFail_WhenNotOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedRestaurantAsync(uow);
        var sut = new CategoryService(uow, _mockLogger.Object);

        var createResult = await sut.CreateCategoryAsync(10, "restaurant_admin", new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" });
        var result = await sut.DeleteCategoryAsync(createResult.Data!.Id, 99, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }
}
