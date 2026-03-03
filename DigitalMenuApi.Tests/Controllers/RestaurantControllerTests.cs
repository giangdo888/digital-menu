using DigitalMenuApi.Controllers;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DigitalMenuApi.Tests.Controllers;

public class RestaurantControllerTests
{
    private readonly Mock<IRestaurantService> _mockRestaurantService;
    private readonly RestaurantController _sut; // System Under Test

    public RestaurantControllerTests()
    {
        _mockRestaurantService = new Mock<IRestaurantService>();
        _sut = new RestaurantController(_mockRestaurantService.Object);

        // Mock the User Claims to avoid NullReferenceExceptions when the controller calls GetCurrentUserId()
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10"),
            new Claim(ClaimTypes.Role, "restaurant_admin")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // If the underlying service says "Yep, here are the user's restaurants",
    // the controller's only job is to wrap that data in a shiny HTTP 200 OK wrapper
    // and send it back to the frontend.
    [Fact]
    public async Task GetMyRestaurants_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var fakeData = new List<RestaurantResponse>
        {
            new RestaurantResponse { Id = 100, Name = "Test Restaurant" }
        };

        // We know from the mocked claims above that GetCurrentUserId() will return 10
        _mockRestaurantService
            .Setup(s => s.GetMyRestaurantsAsync(10))
            .ReturnsAsync(Result<IEnumerable<RestaurantResponse>>.Success(fakeData));

        // Act
        var actionResult = await _sut.GetMyRestaurants();

        // Assert
        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeData);
    }

    // Sometimes things go wrong (e.g., the user forgot to name the restaurant).
    // The service will spit back a failure result. The controller needs to catch that
    // and correctly translate it into an HTTP 400 Bad Request so the frontend knows something is wrong.
    [Fact]
    public async Task CreateRestaurant_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var request = new CreateRestaurantRequest { Name = "Bad Restaurant", Address = "123" };

        _mockRestaurantService
            .Setup(s => s.CreateRestaurantAsync(10, request))
            .ReturnsAsync(Result<RestaurantResponse>.Failure("Invalid data", 400));

        // Act
        var actionResult = await _sut.CreateRestaurant(request);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(400);

        // Verify it returned the error object
        var responseValue = objectResult.Value;
        var errorProperty = responseValue?.GetType().GetProperty("error")?.GetValue(responseValue, null);
        errorProperty.Should().Be("Invalid data");
    }

    // If a user tries to rename a restaurant they absolutely do not own,
    // the service throws a Forbidden error. This test ensures the controller 
    // correctly translates that into an HTTP 403 Forbidden status code.
    [Fact]
    public async Task UpdateRestaurant_ShouldReturnForbidden_WhenUserDoesNotOwnIt()
    {
        // Arrange
        var request = new UpdateRestaurantRequest { Name = "Hacked Name" };

        _mockRestaurantService
            .Setup(s => s.UpdateRestaurantAsync(999, 10, "restaurant_admin", request))
            .ReturnsAsync(Result<RestaurantResponse>.Forbidden("You don't own this"));

        // Act
        var actionResult = await _sut.UpdateRestaurant(999, request);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
    }
}
