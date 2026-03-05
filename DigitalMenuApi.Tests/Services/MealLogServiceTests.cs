using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalMenuApi.Data;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Implementations;
using DigitalMenuApi.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class MealLogServiceTests
{
    private readonly Mock<ILogger<MealLogService>> _mockLogger;

    public MealLogServiceTests()
    {
        _mockLogger = new Mock<ILogger<MealLogService>>();
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        return new UnitOfWork(context);
    }

    private async Task SeedDataAsync(UnitOfWork uow)
    {
        // Seed Category
        var category = new Category
        {
            Id = 1,
            Name = "Lunch",
            RestaurantId = 1,
            Type = "food"
        };
        await uow.Categories.AddAsync(category);

        // Seed Dish
        var dish = new Dish
        {
            Id = 10,
            Name = "Grilled Chicken",
            Price = 12.99M,
            CategoryId = 1,
            Calories = 350,
            ProteinG = 40,
            CarbsG = 5,
            FatG = 15,
            ImageUrl = "test.png"
        };
        await uow.Dishes.AddAsync(dish);

        // Seed User
        var user = new User
        {
            Id = 100,
            Email = "test@user.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = 3,
            IsActive = true
        };
        await uow.Users.AddAsync(user);

        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateMealLogAsync_ShouldSucceed_WhenDishExists()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var request = new CreateMealLogRequest
        {
            UserId = 100,
            DishId = 10
        };

        // Act
        var result = await sut.CreateMealLogAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DishId.Should().Be("10");
        result.Data.UserId.Should().Be("100");
        result.Data.Calories.Should().Be("350");
        result.Data.ProteinG.Should().Be("40");

        var dbLog = await uow.MealLogs.Query().FirstOrDefaultAsync(m => m.UserId == 100);
        dbLog.Should().NotBeNull();
        dbLog!.Calories.Should().Be(350);
    }

    [Fact]
    public async Task CreateMealLogAsync_ShouldFail_WhenDishDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new MealLogService(uow, _mockLogger.Object);

        var request = new CreateMealLogRequest
        {
            UserId = 100,
            DishId = 999 // Non-existent dish
        };

        // Act
        var result = await sut.CreateMealLogAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Dish not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetMealLogsByUserIdAsync_ShouldReturnLogs()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });

        // Act
        var result = await sut.GetMealLogsByUserIdAsync(100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMealLogByIdAsync_ShouldReturnLog_WhenOwnedByUser()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        // Act
        var result = await sut.GetMealLogByIdAsync(100, logId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(logId.ToString());
    }

    [Fact]
    public async Task GetMealLogByIdAsync_ShouldFail_WhenLogBelongsToAnotherUser()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        // Act
        var result = await sut.GetMealLogByIdAsync(200, logId); // Different User ID

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Meal log not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateMealLogAsync_ShouldUpdate_WhenValid()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        // Add a second dish to update to
        await uow.Dishes.AddAsync(new Dish
        {
            Id = 11,
            Name = "Salad",
            Price = 8.99M,
            CategoryId = 1,
            Calories = 150,
            ProteinG = 5,
            CarbsG = 20,
            FatG = 2,
            ImageUrl = "salad.png"
        });
        await uow.SaveChangesAsync();

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        var updateRequest = new UpdateMealLogRequest
        {
            Id = logId,
            UserId = 100,
            DishId = 11 // Update to Dish 11
        };

        // Act
        var result = await sut.UpdateMealLogAsync(updateRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.DishId.Should().Be("11");
        result.Data.Calories.Should().Be("150");
    }

    [Fact]
    public async Task UpdateMealLogAsync_ShouldFail_WhenUnauthorized()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        var updateRequest = new UpdateMealLogRequest
        {
            Id = logId,
            UserId = 200, // Unauthorized User
            DishId = 10
        };

        // Act
        var result = await sut.UpdateMealLogAsync(updateRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unauthorized");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task DeleteMealLogAsync_ShouldSucceed_WhenValid()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        // Act
        var result = await sut.DeleteMealLogAsync(100, logId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var checkResult = await sut.GetMealLogByIdAsync(100, logId);
        checkResult.IsFailure.Should().BeTrue(); // Should no longer exist
    }

    [Fact]
    public async Task GetMealLogByIdAsync_ShouldFail_WhenLogDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new MealLogService(uow, _mockLogger.Object);

        // Act
        var result = await sut.GetMealLogByIdAsync(100, 999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Meal log not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateMealLogAsync_ShouldFail_WhenLogDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new MealLogService(uow, _mockLogger.Object);

        var updateRequest = new UpdateMealLogRequest
        {
            Id = 999,
            UserId = 100,
            DishId = 10
        };

        // Act
        var result = await sut.UpdateMealLogAsync(updateRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Meal log not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateMealLogAsync_ShouldFail_WhenDishDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        var updateRequest = new UpdateMealLogRequest
        {
            Id = logId,
            UserId = 100,
            DishId = 999 // Non-existent dish
        };

        // Act
        var result = await sut.UpdateMealLogAsync(updateRequest);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Dish not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteMealLogAsync_ShouldFail_WhenLogDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new MealLogService(uow, _mockLogger.Object);

        // Act
        var result = await sut.DeleteMealLogAsync(100, 999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Meal log not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteMealLogAsync_ShouldFail_WhenUnauthorized()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        await SeedDataAsync(uow);
        var sut = new MealLogService(uow, _mockLogger.Object);

        var createResult = await sut.CreateMealLogAsync(new CreateMealLogRequest { UserId = 100, DishId = 10 });
        int logId = int.Parse(createResult.Data!.Id);

        // Act
        var result = await sut.DeleteMealLogAsync(200, logId); // Different User ID

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unauthorized");
        result.StatusCode.Should().Be(401);
    }
}
