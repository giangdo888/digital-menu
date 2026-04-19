namespace DigitalMenuApi.Services.Implementations;

public partial class SeedService
{
    private static RestaurantSeedData GetBellaItalia() => new()
    {
        OwnerEmail = "mario@restaurant.com", OwnerFirstName = "Mario", OwnerLastName = "Rossi",
        RestaurantName = "Bella Italia",
        Address = "123 Little Italy Street, Melbourne VIC 3000", Phone = "+61 3 9123 4567",
        Description = "Authentic Italian cuisine in the heart of the city.",
        OpeningHours = "Tue-Sun 12pm-10pm",
        LogoUrl = "https://images.unsplash.com/photo-1559339352-11d035aa65de?auto=format&fit=crop&w=1200&q=80",
        Categories = new()
        {
            new() { Name = "Antipasti", Type = "food", Dishes = new()
            {
                D("Bruschetta", 12.50m, "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=400", ("tomato",100),("bread",60),("olive oil",15),("garlic",5)),
                D("Carpaccio di Manzo", 18.00m, "https://images.unsplash.com/photo-1625944230945-1b7dd3b949ab?w=400", ("beef",120),("olive oil",10),("lemon",15)),
                D("Caprese Salad", 14.00m, "https://images.unsplash.com/photo-1608897013039-887f21d8c804?w=400", ("tomato",150),("mozzarella",100),("basil",10)),
                D("Arancini", 13.00m, "https://images.unsplash.com/photo-1595295333158-4742f28fbd85?w=400", ("rice",120),("cheese",40),("egg",30)),
                D("Prosciutto e Melone", 16.00m, "https://images.unsplash.com/photo-1623428187969-5da2dcea5ebf?w=400", ("ham",80),("melon",150)),
            }},
            new() { Name = "Pasta", Type = "food", Dishes = new()
            {
                D("Spaghetti Carbonara", 24.00m, "https://images.unsplash.com/photo-1612874742237-6526221588e3?w=400", ("pasta",150),("egg",60),("bacon",80),("cheese",40)),
                D("Penne Arrabbiata", 20.00m, "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400", ("pasta",150),("tomato",120),("chilli",10),("garlic",5)),
                D("Fettuccine Alfredo", 23.00m, "https://images.unsplash.com/photo-1645112411341-6c4fd023714a?w=400", ("pasta",160),("cream",100),("butter",30),("cheese",50)),
                D("Lasagna", 26.00m, "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=400", ("pasta",120),("beef",150),("tomato",100),("cheese",80)),
                D("Spaghetti Bolognese", 22.00m, "https://images.unsplash.com/photo-1622973536968-3ead9e780960?w=400", ("pasta",150),("beef",120),("tomato",100),("onion",30)),
                D("Pesto Genovese", 21.00m, "https://images.unsplash.com/photo-1473093295043-cdd812d0e601?w=400", ("pasta",150),("basil",30),("cheese",30)),
                D("Ravioli Ricotta", 25.00m, "https://images.unsplash.com/photo-1587740908075-9e245070dfaa?w=400", ("flour",100),("ricotta",80),("spinach",60),("egg",40)),
            }},
            new() { Name = "Pizza", Type = "food", Dishes = new()
            {
                D("Margherita Pizza", 22.00m, "https://images.unsplash.com/photo-1604068549290-dea0e4a305ca?w=400", ("flour",200),("tomato",100),("mozzarella",120)),
                D("Pizza Quattro Formaggi", 26.00m, "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400", ("flour",200),("mozzarella",60),("cheese",90)),
                D("Pizza Diavola", 25.00m, "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400", ("flour",200),("tomato",100),("salami",60),("chilli",10)),
                D("Pizza Prosciutto", 27.00m, "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400", ("flour",200),("tomato",80),("ham",80),("mozzarella",100)),
                D("Calzone", 24.00m, "https://images.unsplash.com/photo-1536964549093-d8de150a39a4?w=400", ("flour",220),("ham",60),("mozzarella",80),("mushroom",40)),
            }},
            new() { Name = "Dolci", Type = "food", Dishes = new()
            {
                D("Tiramisu", 14.00m, "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400", ("mascarpone",100),("coffee",50),("cocoa",10),("egg",40)),
                D("Panna Cotta", 12.00m, "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400", ("cream",150),("sugar",30),("vanilla",5)),
                D("Cannoli", 11.00m, "https://images.unsplash.com/photo-1631206616096-1a09e2fdab23?w=400", ("ricotta",100),("flour",60),("sugar",25)),
                D("Gelato Misto", 10.00m, "https://images.unsplash.com/photo-1567206563064-6f60f40a2b57?w=400", ("cream",120),("sugar",40),("egg",30)),
            }},
            new() { Name = "Beverages", Type = "drink", Dishes = new()
            {
                D("Espresso", 4.50m, "https://images.unsplash.com/photo-1510707577719-ae7c14805e3a?w=400", ("coffee",30)),
                D("Cappuccino", 5.50m, "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=400", ("coffee",30),("milk",150)),
                D("Latte Macchiato", 6.00m, "https://images.unsplash.com/photo-1485808191679-5f86510681a2?w=400", ("coffee",30),("milk",200)),
                D("Aperol Spritz", 16.00m, "https://images.unsplash.com/photo-1560512823-829485b8bf24?w=400", ("wine",90)),
                D("Limoncello", 12.00m, "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400", ("lemon",30),("sugar",20)),
                D("Prosecco", 14.00m, "https://images.unsplash.com/photo-1598306442928-4d90f32c6866?w=400", ("wine",150)),
                D("Negroni", 18.00m, "https://images.unsplash.com/photo-1551751299-1b51cab2694c?w=400"),
                D("Italian Lemonade", 7.00m, "https://images.unsplash.com/photo-1621263764928-df1444c5e859?w=400", ("lemon",60),("sugar",25)),
                D("Sparkling Water", 5.00m, "https://images.unsplash.com/photo-1523362628745-0c100150b504?w=400"),
                D("Chianti (Glass)", 15.00m, "https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=400", ("wine",150)),
            }},
        }
    };
}
