namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateRestaurantRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Address { get; set; }

    [MaxLength(20)]
    [DigitalMenuApi.Validation.OptionalPhone]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [DigitalMenuApi.Validation.OptionalEmail]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? OpeningHours { get; set; }

    [MaxLength(1000)]
    [DigitalMenuApi.Validation.OptionalUrl]
    public string? LogoUrl { get; set; }
}
