namespace DigitalMenuApi.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/restaurants")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
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

    #region Owner Endpoints

    /// <summary>
    /// Create a new restaurant (Owner only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "restaurant_admin")]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        var result = await _restaurantService.CreateRestaurantAsync(GetCurrentUserId(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all my restaurants (Owner)
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = "restaurant_admin")]
    public async Task<IActionResult> GetMyRestaurants()
    {
        var result = await _restaurantService.GetMyRestaurantsAsync(GetCurrentUserId());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get restaurant by ID (Owner sees own, Admin sees all)
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> GetRestaurantById(int id)
    {
        var result = await _restaurantService.GetRestaurantByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update restaurant (Owner updates own, Admin updates any)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] UpdateRestaurantRequest request)
    {
        var result = await _restaurantService.UpdateRestaurantAsync(id, GetCurrentUserId(), GetCurrentUserRole(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Activate restaurant (Owner activates own, Admin activates any)
    /// </summary>
    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> ActivateRestaurant(int id)
    {
        var result = await _restaurantService.ActivateRestaurantAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "Restaurant activated successfully" });
    }

    /// <summary>
    /// Deactivate restaurant (Owner deactivates own, Admin deactivates any)
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "restaurant_admin,system_admin")]
    public async Task<IActionResult> DeactivateRestaurant(int id)
    {
        var result = await _restaurantService.DeactivateRestaurantAsync(id, GetCurrentUserId(), GetCurrentUserRole());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "Restaurant deactivated successfully" });
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all restaurants (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> GetAllRestaurants()
    {
        var result = await _restaurantService.GetAllRestaurantsAsync();

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    #endregion

    #region Public Endpoints

    /// <summary>
    /// Get list of active restaurants (Public - no auth required)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicRestaurantList()
    {
        var result = await _restaurantService.GetPublicRestaurantListAsync();

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get restaurant by slug (Public - no auth required)
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRestaurantBySlug(string slug)
    {
        var result = await _restaurantService.GetRestaurantBySlugAsync(slug);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get full menu by restaurant slug (Public - no auth required)
    /// Returns restaurant info + categories + dishes with nutrition
    /// </summary>
    [HttpGet("public/{slug}/menu")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMenuBySlug(string slug)
    {
        var result = await _restaurantService.GetMenuBySlugAsync(slug);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    #endregion
}
