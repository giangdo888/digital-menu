namespace DigitalMenuApi.DTOs.Responses;

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set;} = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}