namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static RestaurantSeedData GetElRancho() => new()
    {
        OwnerEmail = "carlos@restaurant.com", OwnerFirstName = "Carlos", OwnerLastName = "Garcia",
        RestaurantName = "El Rancho",
        Address = "321 Mexico Street, Perth WA 6000", Phone = "+61 8 9456 7890",
        Description = "Authentic Mexican flavors and premium tequila selection.",
        OpeningHours = "Mon-Sun 12:00pm-11:00pm",
        LogoUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=1200&q=80",
        Categories = new()
        {
            new() { Name = "Aperitivos", Type = "food", Dishes = new()
            {
                D("Guacamole with Chips", 14.00m, "https://images.unsplash.com/photo-1564834724105-918b73d1b9e0?w=400", ("avocado", 150), ("tomato", 50), ("onion", 30), ("lime", 20), ("coriander", 10)),
                D("Queso Fundido", 16.00m, "https://images.unsplash.com/photo-1599974579688-8dbdd335c77f?w=400", ("cheese", 150), ("chorizo", 50)),
                D("Nachos Supremos", 18.00m, "https://images.unsplash.com/photo-1513456852971-30c0b8199d4d?w=400", ("corn", 100), ("cheese", 100), ("beans", 50), ("jalapeno", 20)),
                D("Ceviche", 20.00m, "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=400", ("fish", 150), ("lime", 40), ("tomato", 50), ("onion", 30))
            }},
            new() { Name = "Tacos", Type = "food", Dishes = new()
            {
                D("Tacos al Pastor", 18.00m, "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400", ("pork", 150), ("pineapple", 40), ("onion", 20), ("coriander", 5), ("tortilla", 60)),
                D("Carne Asada Tacos", 19.00m, "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400", ("beef", 150), ("onion", 30), ("coriander", 10), ("tortilla", 60)),
                D("Chicken Tinga Tacos", 17.00m, "https://images.unsplash.com/photo-1599974579688-8dbdd335c77f?w=400", ("chicken", 150), ("tomato", 50), ("onion", 30), ("tortilla", 60)),
                D("Fish Tacos", 20.00m, "https://images.unsplash.com/photo-1512838243191-e81e8c6680a4?w=400", ("fish", 150), ("cabbage", 40), ("lime", 15), ("tortilla", 60)),
                D("Jackfruit Carnitas Tacos", 16.00m, "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400", ("jackfruit", 150), ("onion", 30), ("tortilla", 60))
            }},
            new() { Name = "Main Courses", Type = "food", Dishes = new()
            {
                D("Beef Burrito", 22.00m, "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400", ("beef", 150), ("rice", 80), ("beans", 60), ("cheese", 40), ("tortilla", 80)),
                D("Chicken Enchiladas", 24.00m, "https://images.unsplash.com/photo-1534353436294-08d4dc8c6222?w=400", ("chicken", 150), ("tortilla", 90), ("cheese", 60), ("sauce", 80)),
                D("Fajitas Mix", 28.00m, "https://images.unsplash.com/photo-1594000305417-062e08e68cfb?w=400", ("beef", 100), ("chicken", 100), ("bell pepper", 80), ("onion", 60)),
                D("Chiles Rellenos", 26.00m, "https://images.unsplash.com/photo-1624300629298-e9f0bd5a2cb7?w=400", ("pepper", 150), ("cheese", 80), ("egg", 60), ("tomato", 100)),
                D("Quesadilla Supreme", 20.00m, "https://images.unsplash.com/photo-1618040996328-9d585ec88755?w=400", ("tortilla", 100), ("cheese", 120), ("chicken", 80), ("onion", 30)),
                D("Mexican Rice Bowl", 18.00m, "https://images.unsplash.com/photo-1512838243191-e81e8c6680a4?w=400", ("rice", 150), ("black beans", 80), ("corn", 50), ("avocado", 60), ("tomato", 40))
            }},
            new() { Name = "Postres", Type = "food", Dishes = new()
            {
                D("Churros", 10.00m, "https://images.unsplash.com/photo-1624300629298-e9f0bd5a2cb7?w=400", ("flour", 100), ("sugar", 30), ("cinnamon", 5), ("chocolate", 40)),
                D("Tres Leches Cake", 12.00m, "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400", ("flour", 80), ("milk", 150), ("sugar", 60), ("egg", 60)),
                D("Flan", 9.00m, "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400", ("milk", 100), ("egg", 60), ("sugar", 40), ("vanilla", 5)),
                D("Mexican Brownie", 11.00m, "https://images.unsplash.com/photo-1621236378699-8597faf6a176?w=400", ("flour", 60), ("chocolate", 80), ("sugar", 50), ("cinnamon", 2))
            }},
            new() { Name = "Bebidas", Type = "drink", Dishes = new()
            {
                D("Classic Margarita", 16.00m, "https://images.unsplash.com/photo-1560512823-829485b8bf24?w=400", ("tequila", 45), ("lime", 30)),
                D("Spicy Jalapeño Margarita", 17.00m, "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=400", ("tequila", 45), ("lime", 30), ("jalapeno", 10)),
                D("Paloma", 15.00m, "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400", ("tequila", 40), ("grapefruit", 80)),
                D("Corona Extra", 8.00m, "https://images.unsplash.com/photo-1618885472179-5e474019f2a9?w=400"),
                D("Modelo Especial", 8.00m, "https://images.unsplash.com/photo-1580956973059-43c3d5a4bb11?w=400"),
                D("Michelada", 12.00m, "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400", ("beer", 330), ("lime", 20), ("tomato", 40)),
                D("Horchata", 7.00m, "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=400", ("rice", 50), ("milk", 100), ("cinnamon", 5), ("sugar", 20)),
                D("Agua de Jamaica", 6.00m, "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400", ("sugar", 20)),
                D("Jarritos (Mango)", 6.00m, "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400"),
                D("Mexican Coke", 6.00m, "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400", ("sugar", 35))
            }}
        }
    };
}
