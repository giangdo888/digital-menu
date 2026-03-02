namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateAFCDItemRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Variant { get; set; }

    [Required]
    [Range(0, 10000)]
    public required decimal Calories { get; set; }

    [Required]
    [Range(0, 1000)]
    public required decimal ProteinG { get; set; }

    [Required]
    [Range(0, 1000)]
    public required decimal CarbsG { get; set; }

    [Required]
    [Range(0, 1000)]
    public required decimal FatG { get; set; }

    public string? FullNutritionJson { get; set; }
}
