using DigitalMenuApi.Controllers;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DigitalMenuApi.Tests.Controllers;

public class AFCDItemControllerTests
{
    private readonly Mock<IAFCDItemService> _mockAFCDItemService;
    private readonly Mock<IAFCDImportService> _mockImportService;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AFCDItemController _sut;

    public AFCDItemControllerTests()
    {
        _mockAFCDItemService = new Mock<IAFCDItemService>();
        _mockImportService = new Mock<IAFCDImportService>();
        _mockConfig = new Mock<IConfiguration>();

        _mockConfig.Setup(c => c["AFCDImport:FilePath"]).Returns("test_path.xlsx");

        _sut = new AFCDItemController(
            _mockAFCDItemService.Object,
            _mockImportService.Object,
            _mockConfig.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Role, "system_admin")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Search_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeData = new List<AFCDItemResponse> { new AFCDItemResponse { Id = 1, Name = "Apple" } };

        _mockAFCDItemService.Setup(s => s.SearchAsync("apple", 50))
            .ReturnsAsync(Result<IEnumerable<AFCDItemResponse>>.Success(fakeData));

        var result = await _sut.Search("apple", 50);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeData);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateAFCDItemRequest { Name = "Apple", Calories = 52, ProteinG = 0.3M, CarbsG = 13.8M, FatG = 0.2M };
        var fakeResponse = new AFCDItemResponse { Id = 1, Name = "Apple", Calories = 52 };

        _mockAFCDItemService.Setup(s => s.CreateAsync(request))
            .ReturnsAsync(Result<AFCDItemResponse>.Success(fakeResponse));

        var result = await _sut.Create(request);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenServiceFails()
    {
        var request = new UpdateAFCDItemRequest { Name = "Updated" };

        _mockAFCDItemService.Setup(s => s.UpdateAsync(999, request))
            .ReturnsAsync(Result<AFCDItemResponse>.Failure("Item not found", 404));

        var result = await _sut.Update(999, request);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Import_ShouldReturnOk_WhenServiceSucceeds()
    {
        var fakeResponse = new AFCDImportResult { TotalRows = 100, Imported = 100, Skipped = 0, Failed = 0 };

        _mockImportService.Setup(s => s.ImportFromExcelAsync("test_path.xlsx"))
            .ReturnsAsync(Result<AFCDImportResult>.Success(fakeResponse));

        var result = await _sut.Import();

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(fakeResponse);
    }

    [Fact]
    public async Task Import_ShouldReturnBadRequest_WhenPathNull()
    {
        // Override config for this test
        _mockConfig.Setup(c => c["AFCDImport:FilePath"]).Returns((string?)null);

        var result = await _sut.Import();

        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }
}
