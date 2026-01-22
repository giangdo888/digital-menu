namespace DigitalMenuApi.DTOs.Responses;

public class RestaurantListItemResponse
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Address { get; set; } = string.Empty;
}
