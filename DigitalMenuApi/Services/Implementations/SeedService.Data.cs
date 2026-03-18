namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static List<RestaurantSeedData> GetRestaurantData()
    {
        return new List<RestaurantSeedData>
        {
            GetBellaItalia(),
            GetTokyoGarden(),
            GetCafeParis(),
            GetElRancho(),
            GetGoldenDragon(),
        };
    }

    #region Seed Data Classes

    private class RestaurantSeedData
    {
        public required string OwnerEmail { get; set; }
        public required string OwnerFirstName { get; set; }
        public required string OwnerLastName { get; set; }
        public required string RestaurantName { get; set; }
        public required string Address { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
        public string? OpeningHours { get; set; }
        public List<CategorySeedData> Categories { get; set; } = new();
    }

    private class CategorySeedData
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public List<DishSeedData> Dishes { get; set; } = new();
    }

    private class DishSeedData
    {
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<IngredientSeedData> Ingredients { get; set; } = new();
    }

    private class IngredientSeedData
    {
        public required string SearchTerm { get; set; }
        public required decimal AmountGrams { get; set; }
    }

    #endregion

    // Helper to create ingredient lists concisely
    private static List<IngredientSeedData> Ing(params (string term, decimal grams)[] items) =>
        items.Select(i => new IngredientSeedData { SearchTerm = i.term, AmountGrams = i.grams }).ToList();

    private static DishSeedData D(string name, decimal price, string? img, params (string term, decimal grams)[] ingredients) =>
        new() { Name = name, Price = price, ImageUrl = img, Ingredients = Ing(ingredients) };
}
