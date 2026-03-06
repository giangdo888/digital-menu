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

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoryController _sut;

    public CategoryControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _sut = new CategoryController(_mockCategoryService.Object);

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
    public async Task CreateCategory_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateCategoryRequest { RestaurantId = 1, Name = "Mains", Type = "food" };
        var fakeResponse = new CategoryResponse { Id = 1, RestaurantId = 1, Name = "Mains", Type = "food", DisplayOrder = 0 };

        _mockCategoryService.Setup(s => s.CreateCategoryAsync(10, "restaurant_admin", request))
            .ReturnsAsync(Result<CategoryResponse>.Success(fakeResponse));

        var result = await _sut.CreateCategory(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task GetCategoriesByRestaurant_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeData = new List<CategoryResponse> { new CategoryResponse { Id = 1, RestaurantId = 1, Name = "Mains", Type = "food", DisplayOrder = 0 } };

        _mockCategoryService.Setup(s => s.GetCategoriesByRestaurantAsync(1, 10, "restaurant_admin"))
            .ReturnsAsync(Result<IEnumerable<CategoryResponse>>.Success(fakeData));

        var result = await _sut.GetCategoriesByRestaurant(1);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeData);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnForbidden_WhenServiceFails()
    {
        var request = new UpdateCategoryRequest { Name = "Updated Mains" };

        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(1, 10, "restaurant_admin", request))
            .ReturnsAsync(Result<CategoryResponse>.Forbidden("Not your category"));

        var result = await _sut.UpdateCategory(1, request);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnOk_WhenServiceSucceeds()
    {
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(1, 10, "restaurant_admin"))
            .ReturnsAsync(Result.Success());

        var result = await _sut.DeleteCategory(1);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}
