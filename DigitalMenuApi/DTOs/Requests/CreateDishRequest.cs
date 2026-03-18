namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateDishRequest
{
    [Required]
    public required int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [Range(0, 10000)]
    public required decimal Price { get; set; }

    [MaxLength(1000)]
    [Url]
    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;
}
