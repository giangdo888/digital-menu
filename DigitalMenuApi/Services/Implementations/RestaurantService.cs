namespace DigitalMenuApi.Services.Implementations;

using System.Text.RegularExpressions;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class RestaurantService : IRestaurantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(IUnitOfWork unitOfWork, ILogger<RestaurantService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Owner Operations

    public async Task<Result<RestaurantResponse>> CreateRestaurantAsync(int ownerId, CreateRestaurantRequest request)
    {
        // Generate slug from name
        var baseSlug = GenerateSlug(request.Name);
        var slug = await EnsureUniqueSlugAsync(baseSlug);

        var restaurant = new Restaurant
        {
            UserId = ownerId,
            Name = request.Name,
            Slug = slug,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            Description = request.Description,
            OpeningHours = request.OpeningHours,
            LogoUrl = request.LogoUrl
        };

        await _unitOfWork.Restaurants.AddAsync(restaurant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Restaurant created: {RestaurantId} by owner {OwnerId}", restaurant.Id, ownerId);

        // Reload with user info for response
        var created = await _unitOfWork.Restaurants.Query()
            .Include(r => r.User)
            .Include(r => r.Categories)
            .FirstAsync(r => r.Id == restaurant.Id);

        return Result<RestaurantResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<RestaurantResponse>> GetRestaurantByIdAsync(int restaurantId, int userId, string userRole)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Categories)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return Result<RestaurantResponse>.NotFound("Restaurant not found");
        }

        // Check authorization: owner can see own, admin can see all
        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            _logger.LogWarning("Unauthorized access attempt to restaurant {RestaurantId} by user {UserId}", restaurantId, userId);
            return Result<RestaurantResponse>.Forbidden("You don't have access to this restaurant");
        }

        return Result<RestaurantResponse>.Success(MapToResponse(restaurant));
    }

    public async Task<Result<IEnumerable<RestaurantResponse>>> GetMyRestaurantsAsync(int ownerId)
    {
        var restaurants = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Categories)
            .Where(r => r.UserId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<RestaurantResponse>>.Success(restaurants.Select(MapToResponse));
    }

    public async Task<Result<RestaurantResponse>> UpdateRestaurantAsync(int restaurantId, int userId, string userRole, UpdateRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .Include(r => r.User)
            .Include(r => r.Categories)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return Result<RestaurantResponse>.NotFound("Restaurant not found");
        }

        // Check authorization
        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt to restaurant {RestaurantId} by user {UserId}", restaurantId, userId);
            return Result<RestaurantResponse>.Forbidden("You don't have permission to update this restaurant");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            restaurant.Name = request.Name;
            // Regenerate slug if name changes
            var baseSlug = GenerateSlug(request.Name);
            restaurant.Slug = await EnsureUniqueSlugAsync(baseSlug, restaurant.Id);
        }

        if (!string.IsNullOrEmpty(request.Address))
            restaurant.Address = request.Address;

        if (request.Phone != null)
            restaurant.Phone = request.Phone;

        if (request.Email != null)
            restaurant.Email = request.Email;

        if (request.Description != null)
            restaurant.Description = request.Description;

        if (request.OpeningHours != null)
            restaurant.OpeningHours = request.OpeningHours;

        if (request.LogoUrl != null)
            restaurant.LogoUrl = request.LogoUrl;

        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Restaurant updated: {RestaurantId}", restaurantId);
        return Result<RestaurantResponse>.Success(MapToResponse(restaurant));
    }

    public async Task<Result> ActivateRestaurantAsync(int restaurantId, int userId, string userRole)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant == null)
        {
            return Result.NotFound("Restaurant not found");
        }

        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            _logger.LogWarning("Unauthorized activate attempt to restaurant {RestaurantId} by user {UserId}", restaurantId, userId);
            return Result.Forbidden("You don't have permission to activate this restaurant");
        }

        restaurant.IsActive = true;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Restaurant activated: {RestaurantId}", restaurantId);
        return Result.Success();
    }

    public async Task<Result> DeactivateRestaurantAsync(int restaurantId, int userId, string userRole)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant == null)
        {
            return Result.NotFound("Restaurant not found");
        }

        if (userRole != "system_admin" && restaurant.UserId != userId)
        {
            _logger.LogWarning("Unauthorized deactivate attempt to restaurant {RestaurantId} by user {UserId}", restaurantId, userId);
            return Result.Forbidden("You don't have permission to deactivate this restaurant");
        }

        restaurant.IsActive = false;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Restaurant deactivated: {RestaurantId}", restaurantId);
        return Result.Success();
    }

    #endregion

    #region Admin Operations

    public async Task<Result<IEnumerable<RestaurantResponse>>> GetAllRestaurantsAsync()
    {
        var restaurants = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Categories)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<RestaurantResponse>>.Success(restaurants.Select(MapToResponse));
    }

    #endregion

    #region Public Operations

    public async Task<Result<RestaurantPublicResponse>> GetRestaurantBySlugAsync(string slug)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive);

        if (restaurant == null)
        {
            return Result<RestaurantPublicResponse>.NotFound("Restaurant not found");
        }

        return Result<RestaurantPublicResponse>.Success(MapToPublicResponse(restaurant));
    }

    public async Task<Result<IEnumerable<RestaurantListItemResponse>>> GetPublicRestaurantListAsync()
    {
        var restaurants = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return Result<IEnumerable<RestaurantListItemResponse>>.Success(restaurants.Select(MapToListItemResponse));
    }

    public async Task<Result<MenuResponse>> GetMenuBySlugAsync(string slug)
    {
        var restaurant = await _unitOfWork.Restaurants.Query()
            .AsNoTracking()
            .Include(r => r.Categories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name))
                .ThenInclude(c => c.Dishes.Where(d => d.IsActive).OrderBy(d => d.DisplayOrder).ThenBy(d => d.Name))
                    .ThenInclude(d => d.DishIngredients)
                        .ThenInclude(di => di.AfcdItem)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Slug == slug && r.IsActive);

        if (restaurant == null)
        {
            return Result<MenuResponse>.NotFound("Restaurant not found");
        }

        var menu = new MenuResponse
        {
            Restaurant = MapToPublicResponse(restaurant),
            Categories = restaurant.Categories.Select(c => new MenuCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                DisplayOrder = c.DisplayOrder,
                Dishes = c.Dishes.Select(d => new MenuDishResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Price = d.Price,
                    ImageUrl = d.ImageUrl,
                    DisplayOrder = d.DisplayOrder,
                    Calories = d.Calories,
                    ProteinG = d.ProteinG,
                    CarbsG = d.CarbsG,
                    FatG = d.FatG,
                    Ingredients = d.DishIngredients.Select(di => new MenuIngredientResponse
                    {
                        Name = di.AfcdItem?.Name ?? string.Empty,
                        Variant = di.AfcdItem?.Variant,
                        AmountInGrams = di.Amount
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        return Result<MenuResponse>.Success(menu);
    }

    #endregion

    #region Helpers

    private static string GenerateSlug(string name)
    {
        // Convert to lowercase, replace spaces with hyphens, remove special characters
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");           // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");    // Remove non-alphanumeric (except hyphens)
        slug = Regex.Replace(slug, @"-+", "-");            // Replace multiple hyphens with single
        slug = slug.Trim('-');                              // Remove leading/trailing hyphens
        return slug;
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int? excludeId = null)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await SlugExistsAsync(slug, excludeId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _unitOfWork.Restaurants.Query().Where(r => r.Slug == slug);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    private static RestaurantResponse MapToResponse(Restaurant restaurant)
    {
        return new RestaurantResponse
        {
            Id = restaurant.Id,
            OwnerId = restaurant.UserId,
            OwnerName = $"{restaurant.User?.FirstName} {restaurant.User?.LastName}".Trim(),
            Name = restaurant.Name,
            Slug = restaurant.Slug,
            Address = restaurant.Address,
            Phone = restaurant.Phone,
            Email = restaurant.Email,
            Description = restaurant.Description,
            OpeningHours = restaurant.OpeningHours,
            LogoUrl = restaurant.LogoUrl,
            IsActive = restaurant.IsActive,
            CreatedAt = restaurant.CreatedAt,
            UpdatedAt = restaurant.UpdatedAt,
            CategoryCount = restaurant.Categories?.Count ?? 0
        };
    }

    private static RestaurantPublicResponse MapToPublicResponse(Restaurant restaurant)
    {
        return new RestaurantPublicResponse
        {
            Name = restaurant.Name,
            Slug = restaurant.Slug,
            Address = restaurant.Address,
            Phone = restaurant.Phone,
            Email = restaurant.Email,
            Description = restaurant.Description,
            OpeningHours = restaurant.OpeningHours,
            LogoUrl = restaurant.LogoUrl
        };
    }

    private static RestaurantListItemResponse MapToListItemResponse(Restaurant restaurant)
    {
        return new RestaurantListItemResponse
        {
            Name = restaurant.Name,
            Slug = restaurant.Slug,
            LogoUrl = restaurant.LogoUrl,
            Address = restaurant.Address
        };
    }

    #endregion
}
