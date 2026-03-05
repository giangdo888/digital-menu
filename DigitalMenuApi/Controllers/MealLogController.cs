using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.Services.Interfaces;
using System.Security.Claims;

namespace DigitalMenuApi.Controllers;

[ApiController]
[Route("api/meal-logs")]
[Authorize]
public class MealLogController : ControllerBase
{
    private readonly IMealLogService _mealLogService;

    public MealLogController(IMealLogService mealLogService)
    {
        _mealLogService = mealLogService;
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

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CreateMealLog([FromBody] CreateMealLogRequest request)
    {
        request.UserId = GetCurrentUserId(); // Security: Force the authenticated user's ID
        var result = await _mealLogService.CreateMealLogAsync(request);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> GetMyMealLogs()
    {
        var userId = GetCurrentUserId(); // Security: Only fetch the authenticated user's logs
        var result = await _mealLogService.GetMealLogsByUserIdAsync(userId);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> GetMealLogById(int id)
    {
        var result = await _mealLogService.GetMealLogByIdAsync(GetCurrentUserId(), id);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> UpdateMealLog(int id, [FromBody] UpdateMealLogRequest request)
    {
        request.Id = id; // Standardize with REST path variable
        request.UserId = GetCurrentUserId(); // Security: Force the authenticated user's ID
        var result = await _mealLogService.UpdateMealLogAsync(request);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> DeleteMealLog(int id)
    {
        var result = await _mealLogService.DeleteMealLogAsync(GetCurrentUserId(), id);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(result.Data);
    }
}