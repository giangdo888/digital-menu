namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static RestaurantSeedData GetCafeParis() => new()
    {
        OwnerEmail = "jean@restaurant.com", OwnerFirstName = "Jean", OwnerLastName = "Dupont",
        RestaurantName = "Café Paris",
        Address = "789 French Quarter, Brisbane QLD 4000", Phone = "+61 7 9345 6789",
        Description = "Experience the charm of Parisian cuisine in the heart of Brisbane.",
        OpeningHours = "Wed-Mon 7:00am-10:00pm",
        Categories = new()
        {
            new() { Name = "Entrées", Type = "food", Dishes = new()
            {
                D("French Onion Soup", 14.00m, "https://images.unsplash.com/photo-1633337424640-54087e6fa162?w=400", ("onion", 200), ("beef", 100), ("bread", 40), ("cheese", 50)),
                D("Escargots de Bourgogne", 18.00m, "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=400", ("butter", 40), ("garlic", 15), ("parsley", 10)),
                D("Pâté de Campagne", 16.00m, "https://images.unsplash.com/photo-1611095966601-528241d7c385?w=400", ("pork", 120), ("chicken", 50), ("bread", 60)),
                D("Niçoise Salad", 20.00m, "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=400", ("tuna", 100), ("egg", 60), ("tomato", 80), ("lettuce", 60), ("olive", 20)),
                D("Quiche Lorraine", 15.00m, "https://images.unsplash.com/photo-1601314115162-8e010a307521?w=400", ("flour", 80), ("egg", 120), ("bacon", 60), ("cream", 80))
            }},
            new() { Name = "Plats Principaux", Type = "food", Dishes = new()
            {
                D("Coq au Vin", 34.00m, "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=400", ("chicken", 250), ("wine", 100), ("mushroom", 80), ("bacon", 50)),
                D("Beef Bourguignon", 38.00m, "https://images.unsplash.com/photo-1544378730-8b5afcbba63e?w=400", ("beef", 250), ("wine", 150), ("carrot", 80), ("onion", 60)),
                D("Duck Confit", 36.00m, "https://images.unsplash.com/photo-1512169558917-80775a7c2957?w=400", ("duck", 200), ("potato", 150), ("garlic", 10)),
                D("Steak Frites", 40.00m, "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=400", ("beef", 250), ("potato", 200), ("butter", 30)),
                D("Ratatouille", 26.00m, "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=400", ("eggplant", 100), ("zucchini", 100), ("tomato", 120), ("bell pepper", 80)),
                D("Bouillabaisse", 45.00m, "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=400", ("fish", 200), ("shrimp", 80), ("tomato", 100), ("onion", 50))
            }},
            new() { Name = "Sandwiches & Crepes", Type = "food", Dishes = new()
            {
                D("Croque Monsieur", 18.00m, "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400", ("bread", 100), ("ham", 60), ("cheese", 80), ("butter", 20)),
                D("Croque Madame", 20.00m, "https://images.unsplash.com/photo-1475090169767-40ed8d18f67d?w=400", ("bread", 100), ("ham", 60), ("cheese", 80), ("egg", 60)),
                D("Savory Crepe (Complete)", 16.00m, "https://images.unsplash.com/photo-1519676867240-f03562e64548?w=400", ("flour", 60), ("ham", 40), ("cheese", 50), ("egg", 60)),
                D("Mushroom Crepe", 15.00m, "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=400", ("flour", 60), ("mushroom", 80), ("cheese", 40), ("cream", 30))
            }},
            new() { Name = "Desserts", Type = "food", Dishes = new()
            {
                D("Crème Brûlée", 12.00m, "https://images.unsplash.com/photo-1472555794301-77353b152fb7?w=400", ("cream", 150), ("egg", 60), ("sugar", 40), ("vanilla", 5)),
                D("Macarons (3)", 10.00m, "https://images.unsplash.com/photo-1569864358642-9d1684040f43?w=400", ("almond", 40), ("sugar", 60), ("egg", 30)),
                D("Tarte Tatin", 14.00m, "https://images.unsplash.com/photo-1519676867240-f03562e64548?w=400", ("apple", 150), ("flour", 80), ("butter", 50), ("sugar", 60)),
                D("Profiteroles", 16.00m, "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=400", ("flour", 50), ("ice cream", 100), ("chocolate", 60)),
                D("Chocolate Éclair", 8.00m, "https://images.unsplash.com/photo-1621236378699-8597faf6a176?w=400", ("flour", 40), ("chocolate", 30), ("cream", 50)),
                D("Sweet Crepe (Nutella)", 12.00m, "https://images.unsplash.com/photo-1519676867240-f03562e64548?w=400", ("flour", 60), ("chocolate", 40))
            }},
            new() { Name = "Boissons", Type = "drink", Dishes = new()
            {
                D("Café au Lait", 5.00m, "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=400", ("coffee", 30), ("milk", 150)),
                D("Espresso", 4.00m, "https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=400", ("coffee", 30)),
                D("Chocolat Chaud", 7.00m, "https://images.unsplash.com/photo-1544787219-7f47ccb76574?w=400", ("chocolate", 40), ("milk", 200)),
                D("Kir Royale", 18.00m, "https://images.unsplash.com/photo-1505252585461-04db1eb84625?w=400", ("wine", 120)),
                D("French Martini", 20.00m, "https://images.unsplash.com/photo-1560512823-829485b8bf24?w=400"),
                D("Bordeaux Rouge (Glass)", 16.00m, "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=400", ("wine", 150)),
                D("Chardonnay (Glass)", 15.00m, "https://images.unsplash.com/photo-1506377247377-2a5b3b417ebb?w=400", ("wine", 150)),
                D("Sparkling Water (Perrier)", 6.00m, "https://images.unsplash.com/photo-1523362628745-0c100150b504?w=400"),
                D("Lemonade (Citronnade)", 7.00m, "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400", ("lemon", 50), ("sugar", 20)),
                D("Orangina", 6.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451b0f?w=400", ("orange", 100), ("sugar", 20))
            }}
        }
    };
}
