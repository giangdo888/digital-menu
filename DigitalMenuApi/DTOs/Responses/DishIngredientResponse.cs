namespace DigitalMenuApi.DTOs.Responses;

public class DishIngredientResponse
{
    public int Id { get; set; }
    public int AfcdItemId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string? Variant { get; set; }
    public decimal AmountInGrams { get; set; }

    // Calculated nutrition for this amount
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
