namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class DishService : IDishService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DishService> _logger;

    public DishService(IUnitOfWork unitOfWork, ILogger<DishService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Dish CRUD

    public async Task<Result<DishResponse>> CreateDishAsync(int userId, string userRole, CreateDishRequest request)
    {
        // Verify category exists and user has access
        var category = await _unitOfWork.Categories.Query()
            .AsNoTracking()
            .Include(c => c.Restaurant)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId);

        if (category == null)
        {
            return Result<DishResponse>.NotFound("Category not found");
        }

        if (userRole != "system_admin" && category.Restaurant.UserId != userId)
        {
            return Result<DishResponse>.Forbidden("You don't have access to this restaurant");
        }

        var dish = new Dish
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder
        };

        await _unitOfWork.Dishes.AddAsync(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish created: {DishId} in category {CategoryId}", dish.Id, request.CategoryId);

        // Reload with category info
        var created = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
            .Include(d => d.DishIngredients)
            .FirstAsync(d => d.Id == dish.Id);

        return Result<DishResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<IEnumerable<DishResponse>>> GetDishesByCategoryAsync(int categoryId, int userId, string userRole)
    {
        var category = await _unitOfWork.Categories.Query()
            .AsNoTracking()
            .Include(c => c.Restaurant)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result<IEnumerable<DishResponse>>.NotFound("Category not found");
        }

        if (userRole != "system_admin" && category.Restaurant.UserId != userId)
        {
            return Result<IEnumerable<DishResponse>>.Forbidden("You don't have access to this category");
        }

        var dishes = await _unitOfWork.Dishes.Query()
            .AsNoTracking()
            .Include(d => d.Category)
            .Include(d => d.DishIngredients)
            .Where(d => d.CategoryId == categoryId)
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .ToListAsync();

        return Result<IEnumerable<DishResponse>>.Success(dishes.Select(MapToResponse));
    }

    public async Task<Result<DishResponse>> GetDishByIdAsync(int dishId, int userId, string userRole)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .AsNoTracking()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .Include(d => d.DishIngredients)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result<DishResponse>.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result<DishResponse>.Forbidden("You don't have access to this dish");
        }

        return Result<DishResponse>.Success(MapToResponse(dish));
    }

    public async Task<Result<DishResponse>> UpdateDishAsync(int dishId, int userId, string userRole, UpdateDishRequest request)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .Include(d => d.DishIngredients)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result<DishResponse>.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result<DishResponse>.Forbidden("You don't have permission to update this dish");
        }

        if (!string.IsNullOrEmpty(request.Name))
            dish.Name = request.Name;

        if (request.Price.HasValue)
            dish.Price = request.Price.Value;

        if (request.ImageUrl != null)
            dish.ImageUrl = request.ImageUrl;

        if (request.DisplayOrder.HasValue)
            dish.DisplayOrder = request.DisplayOrder.Value;

        _unitOfWork.Dishes.Update(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish updated: {DishId}", dishId);
        return Result<DishResponse>.Success(MapToResponse(dish));
    }

    public async Task<Result> ActivateDishAsync(int dishId, int userId, string userRole)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to activate this dish");
        }

        dish.IsActive = true;
        _unitOfWork.Dishes.Update(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish activated: {DishId}", dishId);
        return Result.Success();
    }

    public async Task<Result> DeactivateDishAsync(int dishId, int userId, string userRole)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to deactivate this dish");
        }

        dish.IsActive = false;
        _unitOfWork.Dishes.Update(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish deactivated: {DishId}", dishId);
        return Result.Success();
    }

    public async Task<Result> DeleteDishAsync(int dishId, int userId, string userRole)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to delete this dish");
        }

        _unitOfWork.Dishes.Delete(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish deleted: {DishId}", dishId);
        return Result.Success();
    }

    #endregion

    #region Ingredients Management

    public async Task<Result<IEnumerable<DishIngredientResponse>>> GetDishIngredientsAsync(int dishId, int userId, string userRole)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .AsNoTracking()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .Include(d => d.DishIngredients)
                .ThenInclude(di => di.AfcdItem)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result<IEnumerable<DishIngredientResponse>>.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result<IEnumerable<DishIngredientResponse>>.Forbidden("You don't have access to this dish");
        }

        var responses = dish.DishIngredients.Select(MapToIngredientResponse);
        return Result<IEnumerable<DishIngredientResponse>>.Success(responses);
    }

    public async Task<Result<IEnumerable<DishIngredientResponse>>> UpdateDishIngredientsAsync(int dishId, int userId, string userRole, UpdateDishIngredientsRequest request)
    {
        var dish = await _unitOfWork.Dishes.Query()
            .Include(d => d.Category)
                .ThenInclude(c => c.Restaurant)
            .Include(d => d.DishIngredients)
            .FirstOrDefaultAsync(d => d.Id == dishId);

        if (dish == null)
        {
            return Result<IEnumerable<DishIngredientResponse>>.NotFound("Dish not found");
        }

        if (userRole != "system_admin" && dish.Category.Restaurant.UserId != userId)
        {
            return Result<IEnumerable<DishIngredientResponse>>.Forbidden("You don't have permission to update this dish");
        }

        // Validate all AFCD items exist
        var afcdItemIds = request.Ingredients.Select(i => i.AfcdItemId).Distinct().ToList();
        var afcdItems = await _unitOfWork.AFCDItems.Query()
            .AsNoTracking()
            .Where(a => afcdItemIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        if (afcdItems.Count != afcdItemIds.Count)
        {
            return Result<IEnumerable<DishIngredientResponse>>.Failure("One or more AFCD items not found");
        }

        // Remove existing ingredients
        foreach (var existing in dish.DishIngredients.ToList())
        {
            _unitOfWork.DishIngredients.Delete(existing);
        }

        // Add new ingredients
        var newIngredients = new List<DishIngredient>();
        foreach (var item in request.Ingredients)
        {
            var ingredient = new DishIngredient
            {
                DishId = dishId,
                AfcdItemId = item.AfcdItemId,
                Amount = item.AmountInGrams
            };
            await _unitOfWork.DishIngredients.AddAsync(ingredient);
            newIngredients.Add(ingredient);
        }

        // Recalculate dish nutrition
        RecalculateDishNutrition(dish, request.Ingredients, afcdItems);

        _unitOfWork.Dishes.Update(dish);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Dish ingredients updated: {DishId}, {Count} ingredients", dishId, request.Ingredients.Count);

        // Reload with AFCD items for response
        var updatedDish = await _unitOfWork.Dishes.Query()
            .Include(d => d.DishIngredients)
                .ThenInclude(di => di.AfcdItem)
            .FirstAsync(d => d.Id == dishId);

        var responses = updatedDish.DishIngredients.Select(MapToIngredientResponse);
        return Result<IEnumerable<DishIngredientResponse>>.Success(responses);
    }

    private void RecalculateDishNutrition(Dish dish, List<DishIngredientItem> ingredients, Dictionary<int, AFCDItem> afcdItems)
    {
        decimal totalCalories = 0;
        decimal totalProtein = 0;
        decimal totalCarbs = 0;
        decimal totalFat = 0;

        foreach (var ingredient in ingredients)
        {
            var afcdItem = afcdItems[ingredient.AfcdItemId];
            var multiplier = ingredient.AmountInGrams / 100m; // AFCD data is per 100g

            totalCalories += afcdItem.Calories * multiplier;
            totalProtein += afcdItem.ProteinG * multiplier;
            totalCarbs += afcdItem.CarbsG * multiplier;
            totalFat += afcdItem.FatG * multiplier;
        }

        dish.Calories = Math.Round(totalCalories, 1);
        dish.ProteinG = Math.Round(totalProtein, 1);
        dish.CarbsG = Math.Round(totalCarbs, 1);
        dish.FatG = Math.Round(totalFat, 1);
    }

    #endregion

    #region Mappers

    private static DishResponse MapToResponse(Dish dish)
    {
        return new DishResponse
        {
            Id = dish.Id,
            CategoryId = dish.CategoryId,
            CategoryName = dish.Category?.Name ?? string.Empty,
            Name = dish.Name,
            Price = dish.Price,
            ImageUrl = dish.ImageUrl,
            DisplayOrder = dish.DisplayOrder,
            IsActive = dish.IsActive,
            Calories = dish.Calories,
            ProteinG = dish.ProteinG,
            CarbsG = dish.CarbsG,
            FatG = dish.FatG,
            IngredientCount = dish.DishIngredients?.Count ?? 0,
            CreatedAt = dish.CreatedAt
        };
    }

    private static DishIngredientResponse MapToIngredientResponse(DishIngredient di)
    {
        var multiplier = di.Amount / 100m;
        return new DishIngredientResponse
        {
            Id = di.Id,
            AfcdItemId = di.AfcdItemId,
            IngredientName = di.AfcdItem?.Name ?? string.Empty,
            Variant = di.AfcdItem?.Variant,
            AmountInGrams = di.Amount,
            Calories = Math.Round((di.AfcdItem?.Calories ?? 0) * multiplier, 1),
            ProteinG = Math.Round((di.AfcdItem?.ProteinG ?? 0) * multiplier, 1),
            CarbsG = Math.Round((di.AfcdItem?.CarbsG ?? 0) * multiplier, 1),
            FatG = Math.Round((di.AfcdItem?.FatG ?? 0) * multiplier, 1)
        };
    }

    #endregion
}
