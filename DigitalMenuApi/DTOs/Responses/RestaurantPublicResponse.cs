namespace DigitalMenuApi.DTOs.Responses;

public class RestaurantPublicResponse
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? OpeningHours { get; set; }
    public string? LogoUrl { get; set; }
}
