using DigitalMenuApi.Data;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Implementations;
using DigitalMenuApi.Services.Implementations;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DigitalMenuApi.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly INutritionService _nutritionService;

    public UserServiceTests()
    {
        _mockLogger = new Mock<ILogger<UserService>>();
        _nutritionService = new NutritionService(); // Use real nutrition service since it's pure math
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        // Seed basic roles needed for Include(u => u.Role)
        context.Roles.AddRange(
            new Role { Id = 1, Name = "system_admin", Description = "System Admin" },
            new Role { Id = 2, Name = "restaurant_admin", Description = "Restaurant Admin" },
            new Role { Id = 3, Name = "customer", Description = "Customer" }
        );
        context.SaveChanges();

        return new UnitOfWork(context);
    }

    private async Task SeedUserAsync(UnitOfWork uow, int userId = 10, bool isActive = true)
    {
        await uow.Users.AddAsync(new User
        {
            Id = userId,
            Email = $"user{userId}@test.com",
            FirstName = "User",
            LastName = "Test",
            PasswordHash = "hash",
            RoleId = 3,
            IsActive = isActive
        });
        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var result = await sut.GetUserByIdAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be("User");
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var result = await sut.GetUserByIdAsync(99);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldSucceed()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var result = await sut.UpdateUserAsync(10, new UpdateUserRequest { FirstName = "NewName" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be("NewName");
    }

    [Fact]
    public async Task DeactivateUserAsync_ShouldSucceed()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var result = await sut.DeactivateUserAsync(10);

        result.IsSuccess.Should().BeTrue();

        var user = await uow.Users.GetByIdAsync(10);
        user!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateProfileAsync_ShouldSucceed()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var request = new CreateProfileRequest
        {
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            HeightCm = 180,
            CurrentWeightKg = 80,
            BmiGoal = 22,
            ActivityLevel = "sedentary"
        };

        var result = await sut.CreateProfileAsync(10, request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.CurrentWeightKg.Should().Be(80);
        result.Data.Bmi.Should().BeGreaterThan(0); // Nutrition calculation applied

        // Verify initial weight history is created
        var weightLogs = await sut.GetWeightHistoryAsync(10, 10);
        weightLogs.Data.Should().HaveCount(1);
        weightLogs.Data!.First().WeightKg.Should().Be(80);
    }

    [Fact]
    public async Task CreateProfileAsync_ShouldFail_WhenProfileAlreadyExists()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var request = new CreateProfileRequest { Gender = "male", DateOfBirth = new DateOnly(1990, 1, 1), HeightCm = 180, CurrentWeightKg = 80, ActivityLevel = "sedentary" };
        await sut.CreateProfileAsync(10, request);

        // Second attempt
        var result = await sut.CreateProfileAsync(10, request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Profile already exists. Use update instead.");
    }

    [Fact]
    public async Task LogWeightAsync_ShouldSucceed_AndRecordHistory()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        await sut.CreateProfileAsync(10, new CreateProfileRequest { Gender = "male", DateOfBirth = new DateOnly(1990, 1, 1), HeightCm = 180, CurrentWeightKg = 80, ActivityLevel = "sedentary" });

        var result = await sut.LogWeightAsync(10, new LogWeightRequest { WeightKg = 78.5M });

        result.IsSuccess.Should().BeTrue();
        result.Data!.CurrentWeightKg.Should().Be(78.5M);

        var weightLogs = await sut.GetWeightHistoryAsync(10, 10);
        weightLogs.Data.Should().HaveCount(2); // Initial (80) + New (78.5)
    }

    [Fact]
    public async Task GetProfileAsync_ShouldFail_WhenNoProfileExists()
    {
        using var uow = CreateUnitOfWork();
        await SeedUserAsync(uow, 10);
        var sut = new UserService(uow, _nutritionService, _mockLogger.Object);

        var result = await sut.GetProfileAsync(10);

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
    }
}
