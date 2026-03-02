namespace DigitalMenuApi.DTOs.Responses;

public class MenuResponse
{
    public RestaurantPublicResponse Restaurant { get; set; } = null!;
    public List<MenuCategoryResponse> Categories { get; set; } = new();
}

public class MenuCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<MenuDishResponse> Dishes { get; set; } = new();
}

public class MenuDishResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }

    // Nutrition info
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }

    public List<MenuIngredientResponse> Ingredients { get; set; } = new();
}

public class MenuIngredientResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Variant { get; set; }
    public decimal AmountInGrams { get; set; }
}
