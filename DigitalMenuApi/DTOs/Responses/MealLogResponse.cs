namespace DigitalMenuApi.DTOs.Responses;

public class MealLogResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string DishId { get; set; }
    public required string DishName { get; set; }
    public required string ConsumedAt { get; set; }
    public required string Calories { get; set; }
    public required string ProteinG { get; set; }
    public required string CarbsG { get; set; }
    public required string FatG { get; set; }
    public required string CreatedAt { get; set; }
}