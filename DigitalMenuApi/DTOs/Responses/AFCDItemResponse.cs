namespace DigitalMenuApi.DTOs.Responses;

public class AFCDItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Variant { get; set; }

    // Nutrition per 100g
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
