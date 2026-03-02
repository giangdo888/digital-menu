namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateCategoryRequest
{
    [Required]
    public required int RestaurantId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [RegularExpression("^(food|drink)$", ErrorMessage = "Type must be 'food' or 'drink'")]
    public required string Type { get; set; }

    public int DisplayOrder { get; set; } = 0;
}
