namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<bool>> IsEmailExistsAsync(string email);
    Task<Result<AuthResponse>> RefreshTokenAsync(string request);
    Task<Result<bool>> RevokeTokenAsync(string token);
}