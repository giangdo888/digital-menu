namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IRestaurantService
{
    // Owner operations
    Task<Result<RestaurantResponse>> CreateRestaurantAsync(int ownerId, CreateRestaurantRequest request);
    Task<Result<RestaurantResponse>> GetRestaurantByIdAsync(int restaurantId, int userId, string userRole);
    Task<Result<IEnumerable<RestaurantResponse>>> GetMyRestaurantsAsync(int ownerId);
    Task<Result<RestaurantResponse>> UpdateRestaurantAsync(int restaurantId, int userId, string userRole, UpdateRestaurantRequest request);    Task<Result> ActivateRestaurantAsync(int restaurantId, int userId, string userRole);
    Task<Result> DeactivateRestaurantAsync(int restaurantId, int userId, string userRole);

    // Admin operations
    Task<Result<IEnumerable<RestaurantResponse>>> GetAllRestaurantsAsync();

    // Public operations
    Task<Result<RestaurantPublicResponse>> GetRestaurantBySlugAsync(string slug);
    Task<Result<IEnumerable<RestaurantListItemResponse>>> GetPublicRestaurantListAsync();
}
