namespace DigitalMenuApi.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/dishes")]
[Authorize]
public class DishController : ControllerBase
{
    private readonly IDishService _dishService;

    public DishController(IDishService dishService)
    {
        _dishService = dishService;
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

    #region Dish CRUD

    /// <summary>
    /// Create a new dish
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "restaurant_admin")]
    public async Task<IActionResult> CreateDish([FromBody] CreateDishRequest request)
    {
        var result = await _dishService.CreateDishAsync(GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get dishes by category
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetDishesByCategory([FromQuery] int categoryId)
    {
        var result = await _dishService.GetDishesByCategoryAsync(categoryId, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get dish by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetDishById(int id)
    {
        var result = await _dishService.GetDishByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update dish
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> UpdateDish(int id, [FromBody] UpdateDishRequest request)
    {
        var result = await _dishService.UpdateDishAsync(id, GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Activate dish
    /// </summary>
    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> ActivateDish(int id)
    {
        var result = await _dishService.ActivateDishAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "Dish activated successfully" });
    }

    /// <summary>
    /// Deactivate dish
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> DeactivateDish(int id)
    {
        var result = await _dishService.DeactivateDishAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "Dish deactivated successfully" });
    }

    #endregion

    #region Ingredients

    /// <summary>
    /// Get dish ingredients
    /// </summary>
    [HttpGet("{id:int}/ingredients")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetDishIngredients(int id)
    {
        var result = await _dishService.GetDishIngredientsAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update dish ingredients (bulk replace)
    /// </summary>
    [HttpPut("{id:int}/ingredients")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> UpdateDishIngredients(int id, [FromBody] UpdateDishIngredientsRequest request)
    {
        var result = await _dishService.UpdateDishIngredientsAsync(id, GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    #endregion
}
