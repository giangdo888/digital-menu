namespace DigitalMenuApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/afcd-items")]
[Authorize]
public class AFCDItemController : ControllerBase
{
    private readonly IAFCDItemService _afcdItemService;
    private readonly IAFCDImportService _afcdImportService;
    private readonly IConfiguration _configuration;

    public AFCDItemController(
        IAFCDItemService afcdItemService,
        IAFCDImportService afcdImportService,
        IConfiguration configuration)
    {
        _afcdItemService = afcdItemService;
        _afcdImportService = afcdImportService;
        _configuration = configuration;
    }

    /// <summary>
    /// Search AFCD items (for adding ingredients to dishes)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int limit = 50)
    {
        var result = await _afcdItemService.SearchAsync(search, limit);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get AFCD item by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _afcdItemService.GetByIdAsync(id);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Create AFCD item (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> Create([FromBody] CreateAFCDItemRequest request)
    {
        var result = await _afcdItemService.CreateAsync(request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update AFCD item (Admin only)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAFCDItemRequest request)
    {
        var result = await _afcdItemService.UpdateAsync(id, request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete AFCD item (Admin only)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _afcdItemService.DeleteAsync(id);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "AFCD item deleted successfully" });
    }

    /// <summary>
    /// Import AFCD items from Excel file (System Admin only)
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> Import()
    {
        var filePath = _configuration["AFCDImport:FilePath"];

        if (string.IsNullOrEmpty(filePath))
            return BadRequest(new { error = "AFCDImport:FilePath not configured" });

        var result = await _afcdImportService.ImportFromExcelAsync(filePath);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }
}
