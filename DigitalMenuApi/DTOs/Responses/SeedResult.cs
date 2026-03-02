namespace DigitalMenuApi.DTOs.Responses;

public class SeedResult
{
    public int UsersCreated { get; set; }
    public int RestaurantsCreated { get; set; }
    public int CategoriesCreated { get; set; }
    public int DishesCreated { get; set; }
    public int IngredientsLinked { get; set; }
    public List<string> Warnings { get; set; } = new();
}
