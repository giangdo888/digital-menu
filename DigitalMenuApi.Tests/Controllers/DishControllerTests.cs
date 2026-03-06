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

public class DishControllerTests
{
    private readonly Mock<IDishService> _mockDishService;
    private readonly DishController _sut;

    public DishControllerTests()
    {
        _mockDishService = new Mock<IDishService>();
        _sut = new DishController(_mockDishService.Object);

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

    [Fact]
    public async Task CreateDish_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateDishRequest { CategoryId = 1, Name = "Pasta", Price = 12.0M };
        var fakeResponse = new DishResponse { Id = 1, Name = "Pasta", Price = 12.0M, Calories = 500, ProteinG = 20, CarbsG = 60, FatG = 15 };

        _mockDishService.Setup(s => s.CreateDishAsync(10, "restaurant_admin", request))
            .ReturnsAsync(Result<DishResponse>.Success(fakeResponse));

        var result = await _sut.CreateDish(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task GetDishesByCategory_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeData = new List<DishResponse> { new DishResponse { Id = 1, Name = "Pasta", Price = 12.0M, Calories = 500, ProteinG = 20, CarbsG = 60, FatG = 15 } };

        _mockDishService.Setup(s => s.GetDishesByCategoryAsync(1, 10, "restaurant_admin"))
            .ReturnsAsync(Result<IEnumerable<DishResponse>>.Success(fakeData));

        var result = await _sut.GetDishesByCategory(1);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeData);
    }

    [Fact]
    public async Task UpdateDish_ShouldReturnForbidden_WhenServiceFails()
    {
        var request = new UpdateDishRequest { Name = "Updated Pasta" };

        _mockDishService.Setup(s => s.UpdateDishAsync(1, 10, "restaurant_admin", request))
            .ReturnsAsync(Result<DishResponse>.Forbidden("Not your dish"));

        var result = await _sut.UpdateDish(1, request);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateDishIngredients_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new UpdateDishIngredientsRequest { Ingredients = new List<DishIngredientItem>() };
        var fakeResponse = new List<DishIngredientResponse>();

        _mockDishService.Setup(s => s.UpdateDishIngredientsAsync(1, 10, "restaurant_admin", request))
            .ReturnsAsync(Result<IEnumerable<DishIngredientResponse>>.Success(fakeResponse));

        var result = await _sut.UpdateDishIngredients(1, request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}
