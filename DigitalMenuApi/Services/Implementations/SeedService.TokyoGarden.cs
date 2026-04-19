namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static RestaurantSeedData GetTokyoGarden() => new()
    {
        OwnerEmail = "sakura@restaurant.com",
        OwnerFirstName = "Sakura",
        OwnerLastName = "Tanaka",
        RestaurantName = "Tokyo Garden",
        Address = "456 Japan Way, Sydney NSW 2000",
        Phone = "+61 2 9234 5678",
        Description = "Traditional Japanese dishes prepared by master chefs.",
        OpeningHours = "Tue-Sun 12:00pm-9:30pm",
        LogoUrl = "https://images.unsplash.com/photo-1568018508399-e53bc8babdde?auto=format&fit=crop&w=1200&q=80",
        Categories = new()
        {
            new() { Name = "Starters", Type = "food", Dishes = new()
            {
                D("Edamame", 8.00m, "https://images.unsplash.com/photo-1541094371900-53bc3aaef646?w=400", ("soybean", 150)),
                D("Miso Soup", 6.00m, "https://images.unsplash.com/photo-1628108529202-b3dc2f9de0d0?w=400", ("tofu", 50), ("seaweed", 10), ("soybean", 20)),
                D("Gyoza", 12.00m, "https://images.unsplash.com/photo-1541529086526-db283c563270?w=400", ("pork", 80), ("cabbage", 40), ("flour", 50)),
                D("Takoyaki", 14.00m, "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400", ("octopus", 60), ("flour", 80), ("egg", 30)),
                D("Agedashi Tofu", 10.00m, "https://images.unsplash.com/photo-1549488344-1f9b8d2bd1f3?w=400", ("tofu", 150), ("flour", 20)),
            }},
            new() { Name = "Sushi & Sashimi", Type = "food", Dishes = new()
            {
                D("Salmon Nigiri", 16.00m, "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=400", ("rice", 100), ("salmon", 80)),
                D("Tuna Sashimi", 22.00m, "https://images.unsplash.com/photo-1626804475297-4160adeeb4eb?w=400", ("tuna", 150)),
                D("Dragon Roll", 24.00m, "https://images.unsplash.com/photo-1553621042-f6e147245754?w=400", ("rice", 120), ("eel", 60), ("avocado", 40)),
                D("Spicy Tuna Roll", 18.00m, "https://images.unsplash.com/photo-1579584425555-c3ce17f1a0d3?w=400", ("rice", 120), ("tuna", 60), ("seaweed", 10)),
                D("Rainbow Roll", 26.00m, "https://images.unsplash.com/photo-1617196034183-421b4917c92d?w=400", ("rice", 120), ("tuna", 30), ("salmon", 30), ("avocado", 30)),
                D("Sashimi Platter", 35.00m, "https://images.unsplash.com/photo-1534482421-64566f976cfa?w=400", ("tuna", 100), ("salmon", 100), ("octopus", 50))
            }},
            new() { Name = "Main Courses", Type = "food", Dishes = new()
            {
                D("Chicken Teriyaki", 26.00m, "https://images.unsplash.com/photo-1580476262798-bddd9f4b7369?w=400", ("chicken", 200), ("soy sauce", 30), ("sugar", 15), ("ginger", 10)),
                D("Tonkotsu Ramen", 24.00m, "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400", ("pork", 100), ("noodle", 150), ("egg", 60), ("onion", 20)),
                D("Beef Udon", 22.00m, "https://images.unsplash.com/photo-1617093727343-374698b1b08d?w=400", ("beef", 120), ("noodle", 200), ("soy sauce", 20)),
                D("Pork Katsu Curry", 28.00m, "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=400", ("pork", 150), ("rice", 150), ("curry", 100), ("flour", 30)),
                D("Unagi Don", 32.00m, "https://images.unsplash.com/photo-1662916124747-d352b95dedb1?w=400", ("eel", 120), ("rice", 200), ("soy sauce", 20)),
                D("Tempura Udon", 25.00m, "https://images.unsplash.com/photo-1585032226651-759b368d7246?w=400", ("shrimp", 80), ("flour", 40), ("noodle", 200))
            }},
            new() { Name = "Desserts", Type = "food", Dishes = new()
            {
                D("Mochi Ice Cream", 10.00m, "https://images.unsplash.com/photo-1522072124503-4f9fa3a9abac?w=400", ("rice flour", 50), ("sugar", 30), ("ice cream", 80)),
                D("Matcha Cheesecake", 12.00m, "https://images.unsplash.com/photo-1615844445300-4b676f4e3c83?w=400", ("cream cheese", 100), ("sugar", 40), ("butter", 20)),
                D("Dorayaki", 9.00m, "https://images.unsplash.com/photo-1634547902096-7bbde2245b73?w=400", ("flour", 80), ("egg", 60), ("sugar", 40), ("red bean", 60)),
                D("Taiyaki", 11.00m, "https://images.unsplash.com/photo-1603569501538-4e3bbda33ba2?w=400", ("flour", 70), ("red bean", 50), ("sugar", 30))
            }},
            new() { Name = "Drinks", Type = "drink", Dishes = new()
            {
                D("Green Tea", 4.00m, "https://images.unsplash.com/photo-1627435601361-ec25f5b1d0e5?w=400"),
                D("Sencha Tea", 5.00m, "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=400"),
                D("Sake (Hot)", 12.00m, "https://images.unsplash.com/photo-1569450371457-3f3396fc4cdd?w=400"),
                D("Junmai Sake (Cold)", 15.00m, "https://images.unsplash.com/photo-1582260654060-fcd82f2c8d50?w=400"),
                D("Plum Wine (Umeshu)", 14.00m, "https://images.unsplash.com/photo-1615332579040-4b89ea1c4167?w=400", ("plum", 50)),
                D("Asahi Beer", 8.00m, "https://images.unsplash.com/photo-1618885472179-5e474019f2a9?w=400"),
                D("Sapporo Beer", 8.00m, "https://images.unsplash.com/photo-1580956973059-43c3d5a4bb11?w=400"),
                D("Yuzu Spritz", 12.00m, "https://images.unsplash.com/photo-1610425316130-1ef09a904d6f?w=400"),
                D("Calpico", 6.00m, "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=400", ("sugar", 20), ("milk", 50)),
                D("Ramune", 6.00m, "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400", ("sugar", 25))
            }}
        }
    };
}
