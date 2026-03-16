using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DigitalMenuApi.Controllers;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DigitalMenuApi.Tests.Controllers;

public class MealLogControllerTests
{
    private readonly Mock<IMealLogService> _mockMealLogService;
    private readonly MealLogController _sut;

    public MealLogControllerTests()
    {
        _mockMealLogService = new Mock<IMealLogService>();
        _sut = new MealLogController(_mockMealLogService.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "100"), // User ID 100
            new Claim(ClaimTypes.Role, "customer")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateMealLog_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var request = new CreateMealLogRequest { DishId = 10 };
        var fakeResponse = new MealLogResponse
        {
            Id = "1",
            UserId = "100",
            DishId = "10",
            DishName = "Fake Dish",
            Calories = "350",
            ProteinG = "40",
            CarbsG = "5",
            FatG = "15",
            CreatedAt = "2024-01-01"
        };

        _mockMealLogService.Setup(s => s.CreateMealLogAsync(It.IsAny<CreateMealLogRequest>()))
            .ReturnsAsync(Result<MealLogResponse>.Success(fakeResponse));

        // Act
        var actionResult = await _sut.CreateMealLog(request);

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task CreateMealLog_ShouldReturnNotFound_WhenDishDoesNotExist()
    {
        // Arrange
        var request = new CreateMealLogRequest { DishId = 999 };

        _mockMealLogService.Setup(s => s.CreateMealLogAsync(It.IsAny<CreateMealLogRequest>()))
            .ReturnsAsync(Result<MealLogResponse>.Failure("Dish not found", 404));

        // Act
        var actionResult = await _sut.CreateMealLog(request);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);

        var responseValue = objectResult.Value;
        var errorProperty = responseValue?.GetType().GetProperty("error")?.GetValue(responseValue, null);
        errorProperty.Should().Be("Dish not found");
    }

    [Fact]
    public async Task GetMyMealLogs_ShouldReturnOk_WithData()
    {
        // Arrange
        var fakeData = new List<MealLogResponse>
        {
            new MealLogResponse { Id = "1", UserId = "100", DishId = "10", DishName = "Fake Dish", Calories = "350", ProteinG = "40", CarbsG = "5", FatG = "15", CreatedAt = "2024-01-01" }
        };

        _mockMealLogService.Setup(s => s.GetMealLogsByUserIdAsync(100))
            .ReturnsAsync(Result<List<MealLogResponse>>.Success(fakeData));

        // Act
        var actionResult = await _sut.GetMyMealLogs();

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeData);
    }

    [Fact]
    public async Task UpdateMealLog_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var request = new UpdateMealLogRequest { DishId = 11 };
        var fakeResponse = new MealLogResponse
        {
            Id = "1",
            UserId = "100",
            DishId = "11",
            DishName = "Fake Dish",
            Calories = "200",
            ProteinG = "10",
            CarbsG = "10",
            FatG = "5",
            CreatedAt = "2024-01-01"
        };

        _mockMealLogService.Setup(s => s.UpdateMealLogAsync(It.IsAny<UpdateMealLogRequest>()))
            .ReturnsAsync(Result<MealLogResponse>.Success(fakeResponse));

        // Act
        var actionResult = await _sut.UpdateMealLog(1, request);

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task DeleteMealLog_ShouldReturnForbidden_WhenServiceReturnsUnauthorized() // Translates 401 to 401
    {
        // Arrange
        _mockMealLogService.Setup(s => s.DeleteMealLogAsync(100, 999))
            .ReturnsAsync(Result<MealLogResponse>.Failure("Unauthorized", 401));

        // Act
        var actionResult = await _sut.DeleteMealLog(999);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(401);
    }
    [Fact]
    public async Task GetMealLogById_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var fakeResponse = new MealLogResponse
        {
            Id = "1",
            UserId = "100",
            DishId = "10",
            DishName = "Fake Dish",
            Calories = "350",
            ProteinG = "40",
            CarbsG = "5",
            FatG = "15",
            CreatedAt = "2024-01-01"
        };

        _mockMealLogService.Setup(s => s.GetMealLogByIdAsync(100, 1))
            .ReturnsAsync(Result<MealLogResponse>.Success(fakeResponse));

        // Act
        var actionResult = await _sut.GetMealLogById(1);

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task GetMealLogById_ShouldReturnNotFound_WhenLogNotFound()
    {
        // Arrange
        _mockMealLogService.Setup(s => s.GetMealLogByIdAsync(100, 999))
            .ReturnsAsync(Result<MealLogResponse>.Failure("Meal log not found", 404));

        // Act
        var actionResult = await _sut.GetMealLogById(999);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);

        var responseValue = objectResult.Value;
        var errorProperty = responseValue?.GetType().GetProperty("error")?.GetValue(responseValue, null);
        errorProperty.Should().Be("Meal log not found");
    }

    [Fact]
    public async Task UpdateMealLog_ShouldReturnNotFound_WhenLogNotFound()
    {
        // Arrange
        var request = new UpdateMealLogRequest { DishId = 11 };

        _mockMealLogService.Setup(s => s.UpdateMealLogAsync(It.IsAny<UpdateMealLogRequest>()))
            .ReturnsAsync(Result<MealLogResponse>.Failure("Meal log not found", 404));

        // Act
        var actionResult = await _sut.UpdateMealLog(999, request);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);

        var responseValue = objectResult.Value;
        var errorProperty = responseValue?.GetType().GetProperty("error")?.GetValue(responseValue, null);
        errorProperty.Should().Be("Meal log not found");
    }

    [Fact]
    public async Task DeleteMealLog_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var fakeResponse = new MealLogResponse
        {
            Id = "1",
            UserId = "100",
            DishId = "10",
            DishName = "Fake Dish",
            Calories = "350",
            ProteinG = "40",
            CarbsG = "5",
            FatG = "15",
            CreatedAt = "2024-01-01"
        };

        _mockMealLogService.Setup(s => s.DeleteMealLogAsync(100, 1))
            .ReturnsAsync(Result<MealLogResponse>.Success(fakeResponse));

        // Act
        var actionResult = await _sut.DeleteMealLog(1);

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }
}
