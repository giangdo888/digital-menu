namespace DigitalMenuApi.Services.Implementations;

using System.Text.RegularExpressions;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public partial class SeedService : ISeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedService> _logger;

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

            var restaurantAdminRole = await _unitOfWork.Roles.Query()
                .FirstOrDefaultAsync(r => r.Name == "restaurant_admin");

            if (restaurantAdminRole == null)
            {
                return Result<SeedResult>.Failure("restaurant_admin role not found. Run migrations first.", 500);
            }

            var restaurantData = GetRestaurantData();

            foreach (var data in restaurantData)
            {
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
                    LogoUrl = data.LogoUrl,
                    IsActive = true
                };

                await _unitOfWork.Restaurants.AddAsync(restaurant);
                await _unitOfWork.SaveChangesAsync();
                result.RestaurantsCreated++;

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

                    int dishOrder = 0;
                    foreach (var dishData in categoryData.Dishes)
                    {
                        var dish = new Dish
                        {
                            CategoryId = category.Id,
                            Name = dishData.Name,
                            Price = dishData.Price,
                            ImageUrl = dishData.ImageUrl,
                            IsActive = true,
                            DisplayOrder = dishOrder++
                        };

                        await _unitOfWork.Dishes.AddAsync(dish);
                        await _unitOfWork.SaveChangesAsync();
                        result.DishesCreated++;

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
        var item = await _unitOfWork.AFCDItems.Query()
            .FirstOrDefaultAsync(a => a.Name.ToLower() == searchTerm.ToLower());

        if (item != null) return item;

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

        decimal totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0;

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
}
