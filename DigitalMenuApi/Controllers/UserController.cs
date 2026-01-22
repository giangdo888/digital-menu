namespace DigitalMenuApi.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
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

    #region User Endpoints

    /// <summary>
    /// Get current user's info
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _userService.GetCurrentUserAsync(GetCurrentUserId());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update current user's info
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(GetCurrentUserId(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Deactivate a user (Admin only)
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        // Prevent self-deactivation
        if (id == GetCurrentUserId())
            return BadRequest(new { error = "Cannot deactivate your own account" });

        var result = await _userService.DeactivateUserAsync(id);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "User deactivated successfully" });
    }

    /// <summary>
    /// Activate a user (Admin only)
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "system_admin")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var result = await _userService.ActivateUserAsync(id);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(new { message = "User activated successfully" });
    }

    #endregion

    #region Profile Endpoints

    /// <summary>
    /// Get current user's profile with nutrition calculations
    /// </summary>
    [HttpGet("me/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(GetCurrentUserId());

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Create profile for current user
    /// </summary>
    [HttpPost("me/profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
    {
        var result = await _userService.CreateProfileAsync(GetCurrentUserId(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _userService.UpdateProfileAsync(GetCurrentUserId(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    #endregion

    #region Weight Tracking Endpoints

    /// <summary>
    /// Log a new weight entry
    /// </summary>
    [HttpPost("me/weight")]
    public async Task<IActionResult> LogWeight([FromBody] LogWeightRequest request)
    {
        var result = await _userService.LogWeightAsync(GetCurrentUserId(), request);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get weight history
    /// </summary>
    [HttpGet("me/weight")]
    public async Task<IActionResult> GetWeightHistory([FromQuery] int limit = 30)
    {
        var result = await _userService.GetWeightHistoryAsync(GetCurrentUserId(), limit);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Data);
    }

    #endregion
}
