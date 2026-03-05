namespace DigitalMenuApi.Models.Entities;

public class MealLog : BaseEntity
{
    public required int UserId { get; set; }
    public required int DishId { get; set; }

    // Snapshot of nutrition at time of logging (in case dish changes later)
    public required decimal Calories { get; set; }
    public required decimal ProteinG { get; set; }
    public required decimal CarbsG { get; set; }
    public required decimal FatG { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
}
