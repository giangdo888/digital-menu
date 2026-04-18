namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class MealLogService : IMealLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MealLogService> _logger;

    public MealLogService(IUnitOfWork unitOfWork, ILogger<MealLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<MealLogResponse>>> GetMealLogsByUserIdAsync(int userId)
    {
        var mealLogs = await _unitOfWork.MealLogs.Query()
            .AsNoTracking()
            .Include(m => m.Dish)
            .Where(m => m.UserId == userId && m.CreatedAt.Date == DateTime.Today)
            .ToListAsync();

        return Result<List<MealLogResponse>>.Success(mealLogs.Select(MapToResponse).ToList());
    }

    public async Task<Result<MealLogResponse>> GetMealLogByIdAsync(int userId, int mealLogId)
    {
        var mealLog = await _unitOfWork.MealLogs.Query()
            .AsNoTracking()
            .Include(m => m.Dish)
            .FirstOrDefaultAsync(m => m.Id == mealLogId && m.UserId == userId);

        if (mealLog == null)
        {
            return Result<MealLogResponse>.Failure("Meal log not found", 404);
        }

        return Result<MealLogResponse>.Success(MapToResponse(mealLog));
    }

    public async Task<Result<MealLogResponse>> CreateMealLogAsync(CreateMealLogRequest request)
    {
        var dish = await _unitOfWork.Dishes.GetByIdAsync(request.DishId);
        if (dish == null)
        {
            return Result<MealLogResponse>.Failure("Dish not found", 404);
        }

        var mealLog = new MealLog
        {
            UserId = request.UserId,
            DishId = request.DishId,
            Calories = dish.Calories,
            ProteinG = dish.ProteinG,
            CarbsG = dish.CarbsG,
            FatG = dish.FatG
        };

        await _unitOfWork.MealLogs.AddAsync(mealLog);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.MealLogs.Query()
            .Include(m => m.Dish)
            .FirstAsync(m => m.Id == mealLog.Id);

        _logger.LogInformation("Meal log created successfully for user {UserId}", request.UserId);

        return Result<MealLogResponse>.Success(MapToResponse(created));
    }

    public async Task<Result<MealLogResponse>> UpdateMealLogAsync(UpdateMealLogRequest request)
    {
        var existingMealLog = await _unitOfWork.MealLogs.GetByIdAsync(request.Id);
        if (existingMealLog == null)
        {
            return Result<MealLogResponse>.Failure("Meal log not found", 404);
        }

        if (existingMealLog.UserId != request.UserId)
        {
            return Result<MealLogResponse>.Failure("Unauthorized", 401);
        }

        var dish = await _unitOfWork.Dishes.GetByIdAsync(request.DishId);
        if (dish == null)
        {
            return Result<MealLogResponse>.Failure("Dish not found", 404);
        }

        existingMealLog.DishId = request.DishId;
        existingMealLog.Calories = dish.Calories;
        existingMealLog.ProteinG = dish.ProteinG;
        existingMealLog.CarbsG = dish.CarbsG;
        existingMealLog.FatG = dish.FatG;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Meal log updated successfully for user {UserId}", request.UserId);

        var updated = await _unitOfWork.MealLogs.Query()
            .Include(m => m.Dish)
            .FirstAsync(m => m.Id == existingMealLog.Id);

        return Result<MealLogResponse>.Success(MapToResponse(updated));
    }

    public async Task<Result<MealLogResponse>> DeleteMealLogAsync(int userId, int mealLogId)
    {
        var existingMealLog = await _unitOfWork.MealLogs.Query()
            .Include(m => m.Dish)
            .FirstOrDefaultAsync(m => m.Id == mealLogId);
        if (existingMealLog == null)
        {
            return Result<MealLogResponse>.Failure("Meal log not found", 404);
        }

        if (existingMealLog.UserId != userId)
        {
            return Result<MealLogResponse>.Failure("Unauthorized", 401);
        }

        _unitOfWork.MealLogs.Delete(existingMealLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Meal log deleted successfully for user {UserId}", userId);

        return Result<MealLogResponse>.Success(MapToResponse(existingMealLog));
    }

    public async Task<Result<List<DailyNutritionSummaryResponse>>> GetMealLogSummaryAsync(int userId, DateTime startDate, DateTime endDate)
    {
        // Adjust end date to include the full day
        var endOfDay = endDate.Date.AddDays(1).AddTicks(-1);
        var startOfDay = startDate.Date;

        var mealLogs = await _unitOfWork.MealLogs.Query()
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.CreatedAt >= startOfDay && m.CreatedAt <= endOfDay)
            .ToListAsync();

        var summary = mealLogs
            .GroupBy(m => m.CreatedAt.Date)
            .Select(g => new DailyNutritionSummaryResponse
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Calories = g.Sum(m => m.Calories),
                ProteinG = g.Sum(m => m.ProteinG),
                CarbsG = g.Sum(m => m.CarbsG),
                FatG = g.Sum(m => m.FatG)
            })
            .OrderBy(s => s.Date)
            .ToList();

        // Fill in missing dates with zero values
        var resultList = new List<DailyNutritionSummaryResponse>();
        for (var date = startOfDay; date <= endOfDay.Date; date = date.AddDays(1))
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var existing = summary.FirstOrDefault(s => s.Date == dateStr);
            if (existing != null)
            {
                resultList.Add(existing);
            }
            else
            {
                resultList.Add(new DailyNutritionSummaryResponse
                {
                    Date = dateStr,
                    Calories = 0,
                    ProteinG = 0,
                    CarbsG = 0,
                    FatG = 0
                });
            }
        }

        return Result<List<DailyNutritionSummaryResponse>>.Success(resultList);
    }

    private MealLogResponse MapToResponse(MealLog mealLog)
    {
        return new MealLogResponse
        {
            Id = mealLog.Id.ToString(),
            UserId = mealLog.UserId.ToString(),
            DishId = mealLog.DishId.ToString(),
            Calories = mealLog.Calories.ToString(),
            ProteinG = mealLog.ProteinG.ToString(),
            CarbsG = mealLog.CarbsG.ToString(),
            FatG = mealLog.FatG.ToString(),
            CreatedAt = mealLog.CreatedAt.ToString(),
            DishName = mealLog.Dish.Name,
        };
    }

}