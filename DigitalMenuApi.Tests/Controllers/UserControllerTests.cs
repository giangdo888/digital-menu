using DigitalMenuApi.Controllers;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DigitalMenuApi.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UserController _sut;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _sut = new UserController(_mockUserService.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Role, "customer")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeResponse = new UserResponse { Id = 10, Email = "test@test.com", FirstName = "F", LastName = "L", Role = "customer", IsActive = true };

        _mockUserService.Setup(s => s.GetCurrentUserAsync(10))
            .ReturnsAsync(Result<UserResponse>.Success(fakeResponse));

        var result = await _sut.GetCurrentUser();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task UpdateCurrentUser_ShouldReturnBadRequest_WhenServiceFails()
    {
        var request = new UpdateUserRequest { FirstName = "New" };

        _mockUserService.Setup(s => s.UpdateUserAsync(10, request))
            .ReturnsAsync(Result<UserResponse>.Failure("Invalid data", 400));

        var result = await _sut.UpdateCurrentUser(request);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DeactivateUser_ShouldReturnBadRequest_WhenSelfDeactivating()
    {
        // Sut claims user is ID 10
        var result = await _sut.DeactivateUser(10);

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeResponse = new UserProfileResponse { Gender = "male", CurrentWeightKg = 80, Bmi = 24.5M, DailyCaloriesTarget = 2500 };

        _mockUserService.Setup(s => s.GetProfileAsync(10))
            .ReturnsAsync(Result<UserProfileResponse>.Success(fakeResponse));

        var result = await _sut.GetProfile();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task CreateProfile_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateProfileRequest { Gender = "male", DateOfBirth = new DateOnly(1990, 1, 1), HeightCm = 180, CurrentWeightKg = 80 };
        var fakeResponse = new UserProfileResponse { Gender = "male", CurrentWeightKg = 80 };

        _mockUserService.Setup(s => s.CreateProfileAsync(10, request))
            .ReturnsAsync(Result<UserProfileResponse>.Success(fakeResponse));

        var result = await _sut.CreateProfile(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task LogWeight_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new LogWeightRequest { WeightKg = 79M };
        var fakeResponse = new UserProfileResponse { CurrentWeightKg = 79M };

        _mockUserService.Setup(s => s.LogWeightAsync(10, request))
            .ReturnsAsync(Result<UserProfileResponse>.Success(fakeResponse));

        var result = await _sut.LogWeight(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}
