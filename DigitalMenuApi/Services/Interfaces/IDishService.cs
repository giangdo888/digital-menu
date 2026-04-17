namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IDishService
{
    Task<Result<DishResponse>> CreateDishAsync(int userId, string userRole, CreateDishRequest request);
    Task<Result<IEnumerable<DishResponse>>> GetDishesByCategoryAsync(int categoryId, int userId, string userRole);
    Task<Result<DishResponse>> GetDishByIdAsync(int dishId, int userId, string userRole);
    Task<Result<DishResponse>> UpdateDishAsync(int dishId, int userId, string userRole, UpdateDishRequest request);
    Task<Result> ActivateDishAsync(int dishId, int userId, string userRole);
    Task<Result> DeactivateDishAsync(int dishId, int userId, string userRole);
    Task<Result> DeleteDishAsync(int dishId, int userId, string userRole);

    // Ingredients management
    Task<Result<IEnumerable<DishIngredientResponse>>> GetDishIngredientsAsync(int dishId, int userId, string userRole);
    Task<Result<IEnumerable<DishIngredientResponse>>> UpdateDishIngredientsAsync(int dishId, int userId, string userRole, UpdateDishIngredientsRequest request);
}
