namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static RestaurantSeedData GetGoldenDragon() => new()
    {
        OwnerEmail = "ming@restaurant.com", OwnerFirstName = "Ming", OwnerLastName = "Chen",
        RestaurantName = "Golden Dragon",
        Address = "555 Chinatown Lane, Adelaide SA 5000", Phone = "+61 8 9567 8901",
        Description = "Opulent Chinese dining hall featuring premium Dim Sum and seafood.",
        OpeningHours = "Mon-Sun 11:00am-10:00pm",
        LogoUrl = "https://images.unsplash.com/photo-1555126634-323283e090fa?auto=format&fit=crop&w=1200&q=80",
        Categories = new()
        {
            new() { Name = "Dim Sum & Starters", Type = "food", Dishes = new()
            {
                D("Spring Rolls (4)", 12.00m, "https://images.unsplash.com/photo-1544253965-c8bc988220bc?w=400", ("cabbage", 80), ("carrot", 40), ("pork", 60), ("flour", 40)),
                D("Pork Dumplings (6)", 14.00m, "https://images.unsplash.com/photo-1563245455-d16ba694f5fb?w=400", ("pork", 120), ("flour", 80), ("cabbage", 40)),
                D("Shrimp Har Gow (4)", 16.00m, "https://images.unsplash.com/photo-1549488344-1f9b8d2bd1f3?w=400", ("shrimp", 100), ("flour", 50)),
                D("Siu Mai (4)", 15.00m, "https://images.unsplash.com/photo-1584285458023-38e94fa00a18?w=400", ("pork", 80), ("shrimp", 40), ("flour", 50), ("mushroom", 20)),
                D("Hot & Sour Soup", 10.00m, "https://images.unsplash.com/photo-1628108529202-b3dc2f9de0d0?w=400", ("tofu", 60), ("mushroom", 30), ("egg", 30), ("vinegar", 15))
            }},
            new() { Name = "Main Dishes", Type = "food", Dishes = new()
            {
                D("Kung Pao Chicken", 22.00m, "https://images.unsplash.com/photo-1580476262798-bddd9f4b7369?w=400", ("chicken", 200), ("peanut", 40), ("chilli", 15), ("soy sauce", 20)),
                D("Sweet and Sour Pork", 24.00m, "https://images.unsplash.com/photo-1525755662778-989d0524087e?w=400", ("pork", 200), ("pineapple", 50), ("bell pepper", 40), ("sugar", 25), ("vinegar", 20)),
                D("Mapo Tofu", 20.00m, "https://images.unsplash.com/photo-1555546258-29367fc56df2?w=400", ("tofu", 250), ("pork", 80), ("chilli", 20), ("soy sauce", 15)),
                D("Beef with Broccoli", 26.00m, "https://images.unsplash.com/photo-1617093727343-374698b1b08d?w=400", ("beef", 200), ("broccoli", 150), ("soy sauce", 20), ("garlic", 10)),
                D("Peking Duck (Half)", 45.00m, "https://images.unsplash.com/photo-1512169558917-80775a7c2957?w=400", ("duck", 400), ("flour", 100), ("cucumber", 50), ("sauce", 40)),
                D("Ma Po Eggplant", 18.00m, "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=400", ("eggplant", 200), ("pork", 50), ("chilli", 15), ("garlic", 10))
            }},
            new() { Name = "Noodles & Rice", Type = "food", Dishes = new()
            {
                D("Special Fried Rice", 16.00m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=400", ("rice", 250), ("egg", 50), ("shrimp", 40), ("pork", 40), ("peas", 30)),
                D("Vegetarian Fried Rice", 14.00m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=400", ("rice", 250), ("egg", 50), ("carrot", 40), ("peas", 40)),
                D("Chow Mein", 18.00m, "https://images.unsplash.com/photo-1585032226651-759b368d7246?w=400", ("noodle", 200), ("chicken", 80), ("cabbage", 60), ("soy sauce", 20)),
                D("Beef Chow Fun", 22.00m, "https://images.unsplash.com/photo-1552611052-33e04de081de?w=400", ("noodle", 200), ("beef", 120), ("bean sprout", 60), ("soy sauce", 20)),
                D("Singapore Noodles", 20.00m, "https://images.unsplash.com/photo-1585032226651-759b368d7246?w=400", ("noodle", 180), ("shrimp", 60), ("pork", 40), ("curry", 15))
            }},
            new() { Name = "Desserts", Type = "food", Dishes = new()
            {
                D("Mango Pudding", 9.00m, "https://images.unsplash.com/photo-1549488344-1f9b8d2bd1f3?w=400", ("mango", 120), ("cream", 80), ("sugar", 25)),
                D("Sesame Balls (4)", 10.00m, "https://images.unsplash.com/photo-1615844445300-4b676f4e3c83?w=400", ("rice flour", 100), ("red bean", 60), ("sesame", 20), ("sugar", 30)),
                D("Egg Tarts (3)", 12.00m, "https://images.unsplash.com/photo-1601314115162-8e010a307521?w=400", ("flour", 80), ("egg", 100), ("butter", 40), ("sugar", 40)),
                D("Fried Ice Cream", 11.00m, "https://images.unsplash.com/photo-1522072124503-4f9fa3a9abac?w=400", ("ice cream", 150), ("flour", 40), ("oil", 20))
            }},
            new() { Name = "Tea & Drinks", Type = "drink", Dishes = new()
            {
                D("Jasmine Tea (Pot)", 6.00m, "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=400"),
                D("Oolong Tea (Pot)", 6.00m, "https://images.unsplash.com/photo-1627435601361-ec25f5b1d0e5?w=400"),
                D("Pu'er Tea (Pot)", 7.00m, "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?w=400"),
                D("Tsingtao Beer", 8.00m, "https://images.unsplash.com/photo-1618885472179-5e474019f2a9?w=400"),
                D("Plum Wine", 12.00m, "https://images.unsplash.com/photo-1615332579040-4b89ea1c4167?w=400"),
                D("Lychee Martini", 18.00m, "https://images.unsplash.com/photo-1560512823-829485b8bf24?w=400"),
                D("Hong Kong Milk Tea", 7.00m, "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=400", ("tea", 30), ("milk", 100), ("sugar", 20)),
                D("Soy Milk (Sweetened)", 5.00m, "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=400", ("soybean", 50), ("sugar", 20)),
                D("Iced Lemon Tea", 6.00m, "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400", ("tea", 20), ("lemon", 30), ("sugar", 25)),
                D("Aloe Vera Drink", 6.00m, "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400", ("sugar", 20))
            }}
        }
    };
}
