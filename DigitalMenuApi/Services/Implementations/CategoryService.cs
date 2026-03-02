namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(int userId, string userRole, CreateCategoryRequest request)
    {
        // Verify restaurant exists and user has access
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(request.RestaurantId);
        if (restaurant == null)
        {
            return Result<CategoryResponse>.NotFound("Restaurant not found");
        }

        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            return Result<CategoryResponse>.Forbidden("You don't have access to this restaurant");
        }

        var category = new Category
        {
            RestaurantId = request.RestaurantId,
            Name = request.Name,
            Type = request.Type,
            DisplayOrder = request.DisplayOrder
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category created: {CategoryId} for restaurant {RestaurantId}", category.Id, request.RestaurantId);

        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result<IEnumerable<CategoryResponse>>> GetCategoriesByRestaurantAsync(int restaurantId, int userId, string userRole)
    {
        // Verify restaurant exists and user has access
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            return Result<IEnumerable<CategoryResponse>>.NotFound("Restaurant not found");
        }

        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            return Result<IEnumerable<CategoryResponse>>.Forbidden("You don't have access to this restaurant");
        }

        var categories = await _unitOfWork.Categories.Query()
            .Include(c => c.Dishes)
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return Result<IEnumerable<CategoryResponse>>.Success(categories.Select(MapToResponse));
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(int categoryId, int userId, string userRole)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Restaurant)
            .Include(c => c.Dishes)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result<CategoryResponse>.NotFound("Category not found");
        }

        if (userRole != "system_admin" && category.Restaurant.UserId != userId)
        {
            return Result<CategoryResponse>.Forbidden("You don't have access to this category");
        }

        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(int categoryId, int userId, string userRole, UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Restaurant)
            .Include(c => c.Dishes)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result<CategoryResponse>.NotFound("Category not found");
        }

        if (userRole != "system_admin" && category.Restaurant.UserId != userId)
        {
            return Result<CategoryResponse>.Forbidden("You don't have permission to update this category");
        }

        if (!string.IsNullOrEmpty(request.Name))
            category.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Type))
            category.Type = request.Type;

        if (request.DisplayOrder.HasValue)
            category.DisplayOrder = request.DisplayOrder.Value;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category updated: {CategoryId}", categoryId);
        return Result<CategoryResponse>.Success(MapToResponse(category));
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId, int userId, string userRole)
    {
        var category = await _unitOfWork.Categories.Query()
            .Include(c => c.Restaurant)
            .Include(c => c.Dishes)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return Result.NotFound("Category not found");
        }

        if (userRole != "system_admin" && category.Restaurant.UserId != userId)
        {
            return Result.Forbidden("You don't have permission to delete this category");
        }

        if (category.Dishes.Any())
        {
            return Result.Failure("Cannot delete category with dishes. Remove dishes first.");
        }

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category deleted: {CategoryId}", categoryId);
        return Result.Success();
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            RestaurantId = category.RestaurantId,
            Name = category.Name,
            Type = category.Type,
            DisplayOrder = category.DisplayOrder,
            DishCount = category.Dishes?.Count ?? 0,
            CreatedAt = category.CreatedAt
        };
    }
}
