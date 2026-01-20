namespace DigitalMenuApi.Models.Entities;

public class DishIngredient : BaseEntity
{
    public required int DishId { get; set; }
    public required int AfcdItemId { get; set; }
    public required decimal Amount { get; set; }  // Quantity in g/ml

    // Navigation properties
    public Dish Dish { get; set; } = null!;
    public AFCDItem AfcdItem { get; set; } = null!;
}
