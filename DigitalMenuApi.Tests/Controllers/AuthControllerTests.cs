using DigitalMenuApi.Controllers;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DigitalMenuApi.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _sut = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new RegisterRequest { Email = "test@test.com", Password = "pwd", FirstName = "F", LastName = "L", AccountType = "customer" };
        var fakeResponse = new AuthResponse { Token = "token", RefreshToken = "rtoken", Email = "test@test.com", Role = "customer" };

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(Result<AuthResponse>.Success(fakeResponse));

        var result = await _sut.Register(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenServiceFails()
    {
        var request = new RegisterRequest { Email = "test@test.com", Password = "pwd", FirstName = "F", LastName = "L", AccountType = "customer" };

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(Result<AuthResponse>.Failure("Email already exists", 400));

        var result = await _sut.Register(request);

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Email already exists");
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenSuccess()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "pwd" };
        var fakeResponse = new AuthResponse { Token = "token", RefreshToken = "rtoken", Email = "test@test.com", Role = "customer" };

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(Result<AuthResponse>.Success(fakeResponse));

        var result = await _sut.Login(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenFails()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "wrong" };

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(Result<AuthResponse>.Failure("Invalid credentials", 400));

        var result = await _sut.Login(request);

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenSuccess()
    {
        var request = new RefreshTokenRequest { RefreshToken = "rtoken" };
        var fakeResponse = new AuthResponse { Token = "newtoken", RefreshToken = "newrtoken" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync("rtoken"))
            .ReturnsAsync(Result<AuthResponse>.Success(fakeResponse));

        var result = await _sut.RefreshToken(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RevokeToken_ShouldReturnOk_WhenSuccess()
    {
        var request = new RevokeTokenRequest { Token = "rtoken" };

        _mockAuthService.Setup(s => s.RevokeTokenAsync("rtoken"))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _sut.RevokeToken(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}
