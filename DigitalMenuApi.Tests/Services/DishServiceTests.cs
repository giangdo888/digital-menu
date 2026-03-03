using DigitalMenuApi.Data;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Implementations;
using DigitalMenuApi.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class DishServiceTests
{
    private readonly Mock<ILogger<DishService>> _mockLogger;

    public DishServiceTests()
    {
        _mockLogger = new Mock<ILogger<DishService>>();
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        // Pre-seed the database with a user, restaurant, category, and AFCD items for testing

        // 1. Owner User (Id = 10)
        context.Users.Add(new User { Id = 10, Email = "owner@test.com", FirstName = "Owner", LastName = "Test", PasswordHash = "hash", RoleId = 2, IsActive = true });

        // 2. Other User (Id = 20)
        context.Users.Add(new User { Id = 20, Email = "other@test.com", FirstName = "Other", LastName = "User", PasswordHash = "hash", RoleId = 2, IsActive = true });

        // 3. Restaurant owned by User 10
        context.Restaurants.Add(new Restaurant { Id = 100, UserId = 10, Name = "Test Restaurant", Slug = "test", Address = "123", IsActive = true });

        // 4. Category in Restaurant 100
        context.Categories.Add(new Category { Id = 1000, RestaurantId = 100, Name = "Mains", Type = "food", DisplayOrder = 1 });

        // 5. Dish in Category 1000
        context.Dishes.Add(new Dish { Id = 10000, CategoryId = 1000, Name = "Test Dish", Price = 10m, IsActive = true, Calories = 0, ProteinG = 0, CarbsG = 0, FatG = 0 });

        // 6. Valid AFCD Items
        context.AFCDItems.AddRange(
            new AFCDItem { Id = 1, PublicFoodKey = "F1", Name = "Rice", Variant = "white", Calories = 130m, ProteinG = 2.7m, CarbsG = 28m, FatG = 0.3m },
            new AFCDItem { Id = 2, PublicFoodKey = "F2", Name = "Chicken", Variant = "breast", Calories = 165m, ProteinG = 31m, CarbsG = 0m, FatG = 3.6m }
        );

        context.SaveChanges();
        return new UnitOfWork(context);
    }

    // We can't let users create a dish in a menu category that doesn't actually exist.
    // This test ensures the system throws a "Category not found" error if they try.
    [Fact]
    public async Task CreateDishAsync_ShouldFail_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        var request = new CreateDishRequest { CategoryId = 9999, Name = "Ghost Dish", Price = 10m };

        // Act
        var result = await sut.CreateDishAsync(userId: 10, userRole: "restaurant_admin", request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Be("Category not found");
    }

    // This is the core of our multi-tenant security!
    // Imagine Restaurant A and Restaurant B. If the owner of Restaurant B tries to
    // secretly add a dish to Restaurant A's menu, the system MUST block them.
    [Fact]
    public async Task CreateDishAsync_ShouldFail_WhenUserDoesNotOwnRestaurant()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        // CategoryId 1000 belongs to Restaurant 100, which belongs to UserId 10
        var request = new CreateDishRequest { CategoryId = 1000, Name = "Hacked Dish", Price = 10m };

        // Act (Attempting with UserId 20)
        var result = await sut.CreateDishAsync(userId: 20, userRole: "restaurant_admin", request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Error.Should().Be("You don't have access to this restaurant");
    }

    // System Admins are god-mode users. Even if they don't own the restaurant,
    // they need the ability to go in and fix menus for customers if something breaks.
    // This test ensures their role overrides the ownership check.
    [Fact]
    public async Task CreateDishAsync_ShouldSucceed_WhenSystemAdminAttemptsAction()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        var request = new CreateDishRequest { CategoryId = 1000, Name = "Admin Override Dish", Price = 15m };

        // Act (Using irrelevant UserId, but system_admin role)
        var result = await sut.CreateDishAsync(userId: 999, userRole: "system_admin", request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("Admin Override Dish");
    }

    // The standard happy path: The user owns the restaurant, the category exists,
    // and they just want to add a new dish. It should succeed without any drama.
    [Fact]
    public async Task CreateDishAsync_ShouldSucceed_WhenValidOwner()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        var request = new CreateDishRequest { CategoryId = 1000, Name = "Legit Owner Dish", Price = 25m };

        // Act
        var result = await sut.CreateDishAsync(userId: 10, userRole: "restaurant_admin", request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("Legit Owner Dish");
    }

    // When a restaurant adds actual ingredients (like 200g of Rice and 150g of Chicken)
    // to a dish, the system has to go grab the official nutrition facts from the AFCD
    // database, multiply them by the weight, and add them all up.
    // This test makes sure that math is absolutely perfect.
    [Fact]
    public async Task UpdateDishIngredientsAsync_ShouldCalculateNutritionCorrectly()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        var request = new UpdateDishIngredientsRequest
        {
            Ingredients = new List<DishIngredientItem>
            {
                new DishIngredientItem { AfcdItemId = 1, AmountInGrams = 200 }, // 200g of Rice
                new DishIngredientItem { AfcdItemId = 2, AmountInGrams = 150 }  // 150g of Chicken
            }
        };

        // Expected Math (based on seeded AFCD items):
        // Rice (200g) = 130 * 2 = 260 cal, P: 5.4g, C: 56g, F: 0.6g
        // Chicken (150g) = 165 * 1.5 = 247.5 cal, P: 46.5g, C: 0g, F: 5.4g
        // Totals = 507.5 cal, P: 51.9g, C: 56g, F: 6.0g

        // Act
        var result = await sut.UpdateDishIngredientsAsync(dishId: 10000, userId: 10, userRole: "restaurant_admin", request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);

        // Verify the Dish received the updated cached nutrition
        var updatedDish = await uow.Dishes.Query().FirstAsync(d => d.Id == 10000);

        updatedDish.Calories.Should().Be(507.5m);
        updatedDish.ProteinG.Should().Be(51.9m);
        updatedDish.CarbsG.Should().Be(56.0m);
        updatedDish.FatG.Should().Be(6.0m);
    }

    // If the frontend sends us an ingredient ID that doesn't exist in the giant
    // Australian Food database, we have to reject the save, otherwise our math breaks.
    [Fact]
    public async Task UpdateDishIngredientsAsync_ShouldFail_WhenAfcdItemDoesNotExist()
    {
        // Arrange
        using var uow = CreateUnitOfWork();
        var sut = new DishService(uow, _mockLogger.Object);

        var request = new UpdateDishIngredientsRequest
        {
            Ingredients = new List<DishIngredientItem>
            {
                new DishIngredientItem { AfcdItemId = 999, AmountInGrams = 100 } // Fake ingredient
            }
        };

        // Act
        var result = await sut.UpdateDishIngredientsAsync(dishId: 10000, userId: 10, userRole: "restaurant_admin", request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("One or more AFCD items not found");
    }
}
