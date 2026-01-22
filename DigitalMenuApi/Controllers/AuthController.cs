namespace DigitalMenuApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using DigitalMenuApi.Services.Interfaces;
using DigitalMenuApi.DTOs.Requests;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result.IsFailure) {
            return BadRequest(result.Error);
        }
        return Ok(result.Data);
    }

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