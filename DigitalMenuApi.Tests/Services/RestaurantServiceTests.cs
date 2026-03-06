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

public class RestaurantServiceTests
{
    private readonly Mock<ILogger<RestaurantService>> _mockLogger;

    public RestaurantServiceTests()
    {
        _mockLogger = new Mock<ILogger<RestaurantService>>();
    }

    private UnitOfWork CreateUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return new UnitOfWork(context);
    }

    private async Task SeedOwnerAsync(UnitOfWork uow, int ownerId = 10)
    {
        await uow.Users.AddAsync(new User
        {
            Id = ownerId, Email = $"owner{ownerId}@test.com", FirstName = "Owner", LastName = "Test",
            PasswordHash = "hash", RoleId = 2, IsActive = true
        });
        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateRestaurantAsync_ShouldSucceed()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var request = new CreateRestaurantRequest { Name = "My Restaurant", Address = "123 Main St" };
        var result = await sut.CreateRestaurantAsync(10, request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("My Restaurant");
        result.Data.Slug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_ShouldReturn_WhenOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var createResult = await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Test", Address = "St" });
        var result = await sut.GetRestaurantByIdAsync(createResult.Data!.Id, 10, "restaurant_admin");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_ShouldFail_WhenNotOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var createResult = await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Test", Address = "St" });
        var result = await sut.GetRestaurantByIdAsync(createResult.Data!.Id, 99, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_ShouldSucceed_WhenSystemAdmin()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var createResult = await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Test", Address = "St" });
        var result = await sut.GetRestaurantByIdAsync(createResult.Data!.Id, 99, "system_admin");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetMyRestaurantsAsync_ShouldReturnOwnedOnly()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow, 10);
        await SeedOwnerAsync(uow, 20);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "R1", Address = "A1" });
        await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "R2", Address = "A2" });
        await sut.CreateRestaurantAsync(20, new CreateRestaurantRequest { Name = "R3", Address = "A3" });

        var result = await sut.GetMyRestaurantsAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateRestaurantAsync_ShouldSucceed_WhenOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var createResult = await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Old Name", Address = "St" });
        var result = await sut.UpdateRestaurantAsync(createResult.Data!.Id, 10, "restaurant_admin",
            new UpdateRestaurantRequest { Name = "New Name" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateRestaurantAsync_ShouldFail_WhenNotOwned()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var createResult = await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Test", Address = "St" });
        var result = await sut.UpdateRestaurantAsync(createResult.Data!.Id, 99, "restaurant_admin",
            new UpdateRestaurantRequest { Name = "Hacked" });

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetRestaurantByIdAsync_ShouldFail_WhenNotFound()
    {
        using var uow = CreateUnitOfWork();
        var sut = new RestaurantService(uow, _mockLogger.Object);

        var result = await sut.GetRestaurantByIdAsync(999, 10, "restaurant_admin");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Restaurant not found");
    }

    [Fact]
    public async Task GetPublicRestaurantListAsync_ShouldReturnActiveOnly()
    {
        using var uow = CreateUnitOfWork();
        await SeedOwnerAsync(uow);
        var sut = new RestaurantService(uow, _mockLogger.Object);

        await sut.CreateRestaurantAsync(10, new CreateRestaurantRequest { Name = "Active", Address = "St" });

        var result = await sut.GetPublicRestaurantListAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }
}
