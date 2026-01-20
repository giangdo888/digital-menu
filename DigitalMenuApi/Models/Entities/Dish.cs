namespace DigitalMenuApi.Models.Entities;

public class Dish : BaseEntity
{
    public required int CategoryId { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    // Cached nutrition values (recalculated when ingredients change)
    public decimal Calories { get; set; } = 0;
    public decimal ProteinG { get; set; } = 0;
    public decimal CarbsG { get; set; } = 0;
    public decimal FatG { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
}
