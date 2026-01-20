namespace DigitalMenuApi.Models.Entities;

public class AFCDItem : BaseEntity
{
    public required string Name { get; set; }  // Main ingredient name (e.g., "Avocado")
    public string? Variant { get; set; }       // Variant details (e.g., "hass, raw")

    // Core nutrition per 100g (displayed in app)
    public required decimal Calories { get; set; }
    public required decimal ProteinG { get; set; }
    public required decimal CarbsG { get; set; }
    public required decimal FatG { get; set; }

    // Full AFCD data as JSON (for future use)
    public string? FullNutritionJson { get; set; }

    // Navigation properties
    public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
}
