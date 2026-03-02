namespace DigitalMenuApi.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "restaurant_admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateCategoryAsync(GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get categories by restaurant
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetCategoriesByRestaurant([FromQuery] int restaurantId)
    {
        var result = await _categoryService.GetCategoriesByRestaurantAsync(restaurantId, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update category
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete category (hard delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "Category deleted successfully" });
    }
}
