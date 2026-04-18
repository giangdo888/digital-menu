namespace DigitalMenuApi.Services.Interfaces;
using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IMealLogService
{
    Task<Result<MealLogResponse>> CreateMealLogAsync(CreateMealLogRequest request);
    Task<Result<List<MealLogResponse>>> GetMealLogsByUserIdAsync(int userId);
    Task<Result<MealLogResponse>> GetMealLogByIdAsync(int userId, int mealLogId);
    Task<Result<MealLogResponse>> UpdateMealLogAsync(UpdateMealLogRequest request);
    Task<Result<MealLogResponse>> DeleteMealLogAsync(int userId, int mealLogId);
    Task<Result<List<DailyNutritionSummaryResponse>>> GetMealLogSummaryAsync(int userId, DateTime startDate, DateTime endDate);
}
