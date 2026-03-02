namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateAFCDItemRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? Variant { get; set; }

    [Range(0, 10000)]
    public decimal? Calories { get; set; }

    [Range(0, 1000)]
    public decimal? ProteinG { get; set; }

    [Range(0, 1000)]
    public decimal? CarbsG { get; set; }

    [Range(0, 1000)]
    public decimal? FatG { get; set; }

    public string? FullNutritionJson { get; set; }
}
