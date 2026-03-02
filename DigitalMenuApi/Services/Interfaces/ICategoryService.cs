namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> CreateCategoryAsync(int userId, string userRole, CreateCategoryRequest request);
    Task<Result<IEnumerable<CategoryResponse>>> GetCategoriesByRestaurantAsync(int restaurantId, int userId, string userRole);
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(int categoryId, int userId, string userRole);
    Task<Result<CategoryResponse>> UpdateCategoryAsync(int categoryId, int userId, string userRole, UpdateCategoryRequest request);
    Task<Result> DeleteCategoryAsync(int categoryId, int userId, string userRole);
}
