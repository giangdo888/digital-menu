namespace DigitalMenuApi.DTOs.Responses;

public class DailyNutritionSummaryResponse
{
    public required string Date { get; set; } // YYYY-MM-DD
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
