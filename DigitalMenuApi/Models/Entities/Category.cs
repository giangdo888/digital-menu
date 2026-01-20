namespace DigitalMenuApi.Models.Entities;

public class Category : BaseEntity
{
    public required int RestaurantId { get; set; }
    public required string Name { get; set; }  // e.g., "Entree", "Main", "Coffee"
    public required string Type { get; set; }  // "food" / "drink"
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
