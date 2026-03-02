namespace DigitalMenuApi.DTOs.Responses;

public class CategoryResponse
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int DishCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
