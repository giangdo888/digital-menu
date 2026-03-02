namespace DigitalMenuApi.Services.Implementations;

using System.Text.RegularExpressions;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class SeedService : ISeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedService> _logger;

    // Restaurant owner emails for checking if data already exists
    private static readonly string[] OwnerEmails =
    {
        "mario@restaurant.com",
        "sakura@restaurant.com",
        "jean@restaurant.com",
        "carlos@restaurant.com",
        "ming@restaurant.com"
    };

    public SeedService(IUnitOfWork unitOfWork, ILogger<SeedService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SeedResult>> SeedSampleDataAsync()
    {
        var result = new SeedResult();

        // Check if data already exists
        var existingUser = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => OwnerEmails.Contains(u.Email));

        if (existingUser != null)
        {
            _logger.LogWarning("Seed data already exists. User {Email} found.", existingUser.Email);
            return Result<SeedResult>.Failure("Seed data already exists. Clear the database first if you want to re-seed.", 409);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get the restaurant_admin role
            var restaurantAdminRole = await _unitOfWork.Roles.Query()
                .FirstOrDefaultAsync(r => r.Name == "restaurant_admin");

            if (restaurantAdminRole == null)
            {
                return Result<SeedResult>.Failure("restaurant_admin role not found. Run migrations first.", 500);
            }

            // Create restaurant owners and their restaurants
            var restaurantData = GetRestaurantData();

            foreach (var data in restaurantData)
            {
                // Create user
                var user = new User
                {
                    Email = data.OwnerEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FirstName = data.OwnerFirstName,
                    LastName = data.OwnerLastName,
                    RoleId = restaurantAdminRole.Id,
                    IsActive = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                result.UsersCreated++;

                // Create restaurant
                var restaurant = new Restaurant
                {
                    UserId = user.Id,
                    Name = data.RestaurantName,
                    Slug = GenerateSlug(data.RestaurantName),
                    Address = data.Address,
                    Phone = data.Phone,
                    Email = data.OwnerEmail,
                    Description = data.Description,
                    OpeningHours = data.OpeningHours,
                    IsActive = true
                };

                await _unitOfWork.Restaurants.AddAsync(restaurant);
                await _unitOfWork.SaveChangesAsync();
                result.RestaurantsCreated++;

                // Create categories and dishes
                int displayOrder = 0;
                foreach (var categoryData in data.Categories)
                {
                    var category = new Category
                    {
                        RestaurantId = restaurant.Id,
                        Name = categoryData.Name,
                        Type = categoryData.Type,
                        DisplayOrder = displayOrder++
                    };

                    await _unitOfWork.Categories.AddAsync(category);
                    await _unitOfWork.SaveChangesAsync();
                    result.CategoriesCreated++;

                    // Create dishes for this category
                    int dishOrder = 0;
                    foreach (var dishData in categoryData.Dishes)
                    {
                        var dish = new Dish
                        {
                            CategoryId = category.Id,
                            Name = dishData.Name,
                            Price = dishData.Price,
                            IsActive = true,
                            DisplayOrder = dishOrder++
                        };

                        await _unitOfWork.Dishes.AddAsync(dish);
                        await _unitOfWork.SaveChangesAsync();
                        result.DishesCreated++;

                        // Add ingredients
                        foreach (var ingredientData in dishData.Ingredients)
                        {
                            var afcdItem = await FindAfcdItemAsync(ingredientData.SearchTerm);

                            if (afcdItem == null)
                            {
                                result.Warnings.Add($"AFCD item not found for '{ingredientData.SearchTerm}' in dish '{dishData.Name}'");
                                continue;
                            }

                            var dishIngredient = new DishIngredient
                            {
                                DishId = dish.Id,
                                AfcdItemId = afcdItem.Id,
                                Amount = ingredientData.AmountGrams
                            };

                            await _unitOfWork.DishIngredients.AddAsync(dishIngredient);
                            result.IngredientsLinked++;
                        }

                        await _unitOfWork.SaveChangesAsync();

                        // Recalculate dish nutrition
                        await RecalculateDishNutritionAsync(dish.Id);
                    }
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Seed completed: {Users} users, {Restaurants} restaurants, {Categories} categories, {Dishes} dishes, {Ingredients} ingredients",
                result.UsersCreated, result.RestaurantsCreated, result.CategoriesCreated, result.DishesCreated, result.IngredientsLinked);

            return Result<SeedResult>.Success(result);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to seed sample data");
            return Result<SeedResult>.Failure($"Failed to seed data: {ex.Message}", 500);
        }
    }

    private async Task<AFCDItem?> FindAfcdItemAsync(string searchTerm)
    {
        // Try exact match first (case-insensitive)
        var item = await _unitOfWork.AFCDItems.Query()
            .FirstOrDefaultAsync(a => a.Name.ToLower() == searchTerm.ToLower());

        if (item != null) return item;

        // Try contains match
        item = await _unitOfWork.AFCDItems.Query()
            .FirstOrDefaultAsync(a => a.Name.ToLower().Contains(searchTerm.ToLower()));

        return item;
    }

    private async Task RecalculateDishNutritionAsync(int dishId)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.DishIngredients)
                .ThenInclude(di => di.AfcdItem)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null) return;

        decimal totalCalories = 0;
        decimal totalProtein = 0;
        decimal totalCarbs = 0;
        decimal totalFat = 0;

        foreach (var ingredient in dish.DishIngredients)
        {
            if (ingredient.AfcdItem == null) continue;

            var multiplier = ingredient.Amount / 100m;
            totalCalories += ingredient.AfcdItem.Calories * multiplier;
            totalProtein += ingredient.AfcdItem.ProteinG * multiplier;
            totalCarbs += ingredient.AfcdItem.CarbsG * multiplier;
            totalFat += ingredient.AfcdItem.FatG * multiplier;
        }

        dish.Calories = Math.Round(totalCalories, 1);
        dish.ProteinG = Math.Round(totalProtein, 1);
        dish.CarbsG = Math.Round(totalCarbs, 1);
        dish.FatG = Math.Round(totalFat, 1);

        _unitOfWork.Dishes.Update(dish);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private static List<RestaurantSeedData> GetRestaurantData()
    {
        return new List<RestaurantSeedData>
        {
            // 1. Bella Italia (Italian)
            new RestaurantSeedData
            {
                OwnerEmail = "mario@restaurant.com",
                OwnerFirstName = "Mario",
                OwnerLastName = "Rossi",
                RestaurantName = "Bella Italia",
                Address = "123 Little Italy Street, Melbourne VIC 3000",
                Phone = "+61 3 9123 4567",
                Description = "Authentic Italian cuisine with recipes passed down through generations.",
                OpeningHours = "Mon-Sun 11:00am-10:00pm",
                Categories = new List<CategorySeedData>
                {
                    new CategorySeedData
                    {
                        Name = "Antipasti",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Bruschetta",
                                Price = 12.50m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "tomato", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "bread", AmountGrams = 60 },
                                    new IngredientSeedData { SearchTerm = "olive oil", AmountGrams = 15 },
                                    new IngredientSeedData { SearchTerm = "garlic", AmountGrams = 5 },
                                    new IngredientSeedData { SearchTerm = "basil", AmountGrams = 5 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Pasta",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Spaghetti Carbonara",
                                Price = 24.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "pasta", AmountGrams = 150 },
                                    new IngredientSeedData { SearchTerm = "egg", AmountGrams = 60 },
                                    new IngredientSeedData { SearchTerm = "bacon", AmountGrams = 80 },
                                    new IngredientSeedData { SearchTerm = "cheese", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "pepper", AmountGrams = 2 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Pizza",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Margherita Pizza",
                                Price = 22.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "flour", AmountGrams = 200 },
                                    new IngredientSeedData { SearchTerm = "tomato", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "mozzarella", AmountGrams = 120 },
                                    new IngredientSeedData { SearchTerm = "basil", AmountGrams = 10 },
                                    new IngredientSeedData { SearchTerm = "olive oil", AmountGrams = 20 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Dolci",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Tiramisu",
                                Price = 14.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "mascarpone", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "coffee", AmountGrams = 50 },
                                    new IngredientSeedData { SearchTerm = "cocoa", AmountGrams = 10 },
                                    new IngredientSeedData { SearchTerm = "egg", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 30 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Beverages",
                        Type = "drink",
                        Dishes = new List<DishSeedData>()
                    }
                }
            },

            // 2. Tokyo Garden (Japanese)
            new RestaurantSeedData
            {
                OwnerEmail = "sakura@restaurant.com",
                OwnerFirstName = "Sakura",
                OwnerLastName = "Tanaka",
                RestaurantName = "Tokyo Garden",
                Address = "456 Japan Way, Sydney NSW 2000",
                Phone = "+61 2 9234 5678",
                Description = "Traditional Japanese dishes prepared by master chefs.",
                OpeningHours = "Tue-Sun 12:00pm-9:30pm",
                Categories = new List<CategorySeedData>
                {
                    new CategorySeedData
                    {
                        Name = "Starters",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Edamame",
                                Price = 8.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "soybean", AmountGrams = 150 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Sushi",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Salmon Nigiri",
                                Price = 16.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "rice", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "salmon", AmountGrams = 80 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Main Courses",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Chicken Teriyaki",
                                Price = 26.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "chicken", AmountGrams = 200 },
                                    new IngredientSeedData { SearchTerm = "soy sauce", AmountGrams = 30 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 15 },
                                    new IngredientSeedData { SearchTerm = "ginger", AmountGrams = 10 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Desserts",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Mochi Ice Cream",
                                Price = 10.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "rice flour", AmountGrams = 50 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 30 },
                                    new IngredientSeedData { SearchTerm = "ice cream", AmountGrams = 80 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Drinks",
                        Type = "drink",
                        Dishes = new List<DishSeedData>()
                    }
                }
            },

            // 3. Café Paris (French)
            new RestaurantSeedData
            {
                OwnerEmail = "jean@restaurant.com",
                OwnerFirstName = "Jean",
                OwnerLastName = "Dupont",
                RestaurantName = "Café Paris",
                Address = "789 French Quarter, Brisbane QLD 4000",
                Phone = "+61 7 9345 6789",
                Description = "Experience the charm of Parisian cuisine in the heart of Brisbane.",
                OpeningHours = "Wed-Mon 7:00am-10:00pm",
                Categories = new List<CategorySeedData>
                {
                    new CategorySeedData
                    {
                        Name = "Entrées",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "French Onion Soup",
                                Price = 14.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "onion", AmountGrams = 200 },
                                    new IngredientSeedData { SearchTerm = "beef", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "bread", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "cheese", AmountGrams = 50 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Plats Principaux",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Coq au Vin",
                                Price = 34.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "chicken", AmountGrams = 250 },
                                    new IngredientSeedData { SearchTerm = "wine", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "mushroom", AmountGrams = 80 },
                                    new IngredientSeedData { SearchTerm = "bacon", AmountGrams = 50 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Desserts",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Crème Brûlée",
                                Price = 12.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "cream", AmountGrams = 150 },
                                    new IngredientSeedData { SearchTerm = "egg", AmountGrams = 60 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "vanilla", AmountGrams = 5 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Boissons",
                        Type = "drink",
                        Dishes = new List<DishSeedData>()
                    }
                }
            },

            // 4. El Rancho (Mexican)
            new RestaurantSeedData
            {
                OwnerEmail = "carlos@restaurant.com",
                OwnerFirstName = "Carlos",
                OwnerLastName = "Garcia",
                RestaurantName = "El Rancho",
                Address = "321 Mexico Street, Perth WA 6000",
                Phone = "+61 8 9456 7890",
                Description = "Vibrant Mexican flavors and authentic recipes from Oaxaca.",
                OpeningHours = "Mon-Sun 11:30am-11:00pm",
                Categories = new List<CategorySeedData>
                {
                    new CategorySeedData
                    {
                        Name = "Aperitivos",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Guacamole",
                                Price = 14.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "avocado", AmountGrams = 150 },
                                    new IngredientSeedData { SearchTerm = "tomato", AmountGrams = 50 },
                                    new IngredientSeedData { SearchTerm = "onion", AmountGrams = 30 },
                                    new IngredientSeedData { SearchTerm = "lime", AmountGrams = 20 },
                                    new IngredientSeedData { SearchTerm = "coriander", AmountGrams = 10 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Tacos",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Tacos al Pastor",
                                Price = 18.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "pork", AmountGrams = 150 },
                                    new IngredientSeedData { SearchTerm = "pineapple", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "onion", AmountGrams = 20 },
                                    new IngredientSeedData { SearchTerm = "coriander", AmountGrams = 5 },
                                    new IngredientSeedData { SearchTerm = "tortilla", AmountGrams = 60 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Burritos",
                        Type = "food",
                        Dishes = new List<DishSeedData>()
                    },
                    new CategorySeedData
                    {
                        Name = "Postres",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Churros",
                                Price = 10.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "flour", AmountGrams = 100 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 30 },
                                    new IngredientSeedData { SearchTerm = "cinnamon", AmountGrams = 5 },
                                    new IngredientSeedData { SearchTerm = "chocolate", AmountGrams = 40 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Bebidas",
                        Type = "drink",
                        Dishes = new List<DishSeedData>()
                    }
                }
            },

            // 5. Golden Dragon (Chinese)
            new RestaurantSeedData
            {
                OwnerEmail = "ming@restaurant.com",
                OwnerFirstName = "Ming",
                OwnerLastName = "Chen",
                RestaurantName = "Golden Dragon",
                Address = "555 Chinatown Lane, Adelaide SA 5000",
                Phone = "+61 8 9567 8901",
                Description = "Authentic Cantonese and Sichuan cuisine in a traditional setting.",
                OpeningHours = "Mon-Sun 11:00am-10:30pm",
                Categories = new List<CategorySeedData>
                {
                    new CategorySeedData
                    {
                        Name = "Dim Sum",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Spring Rolls",
                                Price = 12.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "cabbage", AmountGrams = 80 },
                                    new IngredientSeedData { SearchTerm = "carrot", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "pork", AmountGrams = 60 },
                                    new IngredientSeedData { SearchTerm = "flour", AmountGrams = 40 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Noodles",
                        Type = "food",
                        Dishes = new List<DishSeedData>()
                    },
                    new CategorySeedData
                    {
                        Name = "Rice Dishes",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Kung Pao Chicken",
                                Price = 22.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "chicken", AmountGrams = 200 },
                                    new IngredientSeedData { SearchTerm = "peanut", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "chilli", AmountGrams = 15 },
                                    new IngredientSeedData { SearchTerm = "soy sauce", AmountGrams = 20 }
                                }
                            },
                            new DishSeedData
                            {
                                Name = "Fried Rice",
                                Price = 16.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "rice", AmountGrams = 250 },
                                    new IngredientSeedData { SearchTerm = "egg", AmountGrams = 50 },
                                    new IngredientSeedData { SearchTerm = "peas", AmountGrams = 40 },
                                    new IngredientSeedData { SearchTerm = "carrot", AmountGrams = 30 },
                                    new IngredientSeedData { SearchTerm = "soy sauce", AmountGrams = 15 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Desserts",
                        Type = "food",
                        Dishes = new List<DishSeedData>
                        {
                            new DishSeedData
                            {
                                Name = "Mango Pudding",
                                Price = 9.00m,
                                Ingredients = new List<IngredientSeedData>
                                {
                                    new IngredientSeedData { SearchTerm = "mango", AmountGrams = 120 },
                                    new IngredientSeedData { SearchTerm = "cream", AmountGrams = 80 },
                                    new IngredientSeedData { SearchTerm = "sugar", AmountGrams = 25 }
                                }
                            }
                        }
                    },
                    new CategorySeedData
                    {
                        Name = "Tea",
                        Type = "drink",
                        Dishes = new List<DishSeedData>()
                    }
                }
            }
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
        public List<IngredientSeedData> Ingredients { get; set; } = new();
    }

    private class IngredientSeedData
    {
        public required string SearchTerm { get; set; }
        public required decimal AmountGrams { get; set; }
    }

    #endregion
}
