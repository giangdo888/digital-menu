namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateCategoryRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [RegularExpression("^(food|drink)$", ErrorMessage = "Type must be 'food' or 'drink'")]
    public string? Type { get; set; }

    public int? DisplayOrder { get; set; }
}
