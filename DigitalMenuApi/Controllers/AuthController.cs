namespace DigitalMenuApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.Services.Interfaces;
using DigitalMenuApi.DTOs.Requests;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Login a user
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Refresh a token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Revoke a token
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _authService.RevokeTokenAsync(request.Token);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }
}