namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateRestaurantRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [DigitalMenuApi.Validation.OptionalPhone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [DigitalMenuApi.Validation.OptionalEmail]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? OpeningHours { get; set; }

    [MaxLength(1000)]
    [DigitalMenuApi.Validation.OptionalUrl]
    public string? LogoUrl { get; set; }
}
