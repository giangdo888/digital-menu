namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INutritionService _nutritionService;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, INutritionService nutritionService, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _nutritionService = nutritionService;
        _logger = logger;
    }

    #region User Operations

    public async Task<Result<UserResponse>> GetCurrentUserAsync(int userId)
    {
        return await GetUserByIdAsync(userId);
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Result<UserResponse>.NotFound("User not found");
        }

        return Result<UserResponse>.Success(MapToUserResponse(user));
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var responses = users.Select(MapToUserResponse);
        return Result<IEnumerable<UserResponse>>.Success(responses);
    }

    public async Task<Result<UserResponse>> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("Update failed: User not found {UserId}", userId);
            return Result<UserResponse>.NotFound("User not found");
        }

        // Check email uniqueness if changing
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _unitOfWork.Users.Query().AsNoTracking().AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Update failed: Email {Email} already exists", request.Email);
                return Result<UserResponse>.Failure("Email already in use");
            }
            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User updated: {UserId}", userId);
        return Result<UserResponse>.Success(MapToUserResponse(user));
    }

    public async Task<Result> DeactivateUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        user.IsActive = false;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User deactivated: {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> ActivateUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        user.IsActive = true;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User activated: {UserId}", userId);
        return Result.Success();
    }

    #endregion

    #region Profile Operations

    public async Task<Result<UserProfileResponse>> GetProfileAsync(int userId)
    {
        var profile = await _unitOfWork.UserProfiles.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            _logger.LogDebug("Profile not found for user: {UserId}", userId);
            return Result<UserProfileResponse>.NotFound("Profile not found. Please create a profile first.");
        }

        return Result<UserProfileResponse>.Success(MapToProfileResponse(profile));
    }

    public async Task<Result<UserProfileResponse>> CreateProfileAsync(int userId, CreateProfileRequest request)
    {
        // Check if profile already exists
        var existingProfile = await _unitOfWork.UserProfiles.Query()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId);

        if (existingProfile)
        {
            _logger.LogWarning("Create profile failed: Profile already exists for {UserId}", userId);
            return Result<UserProfileResponse>.Failure("Profile already exists. Use update instead.");
        }

        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<UserProfileResponse>.NotFound("User not found");
        }

        var profile = new UserProfile
        {
            UserId = userId,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            HeightCm = request.HeightCm,
            CurrentWeightKg = request.CurrentWeightKg,
            BmiGoal = request.BmiGoal,
            ActivityLevel = request.ActivityLevel,
            LastWeightUpdate = DateTime.UtcNow
        };

        await _unitOfWork.UserProfiles.AddAsync(profile);

        // Also create initial weight history entry
        var weightEntry = new WeightHistory
        {
            UserId = userId,
            WeightKg = request.CurrentWeightKg,
            RecordedAt = DateTime.UtcNow
        };
        await _unitOfWork.WeightHistories.AddAsync(weightEntry);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile created for user: {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToProfileResponse(profile));
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var profile = await _unitOfWork.UserProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            _logger.LogWarning("Update profile failed: Profile not found for {UserId}", userId);
            return Result<UserProfileResponse>.NotFound("Profile not found. Please create a profile first.");
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Gender))
            profile.Gender = request.Gender;

        if (request.DateOfBirth.HasValue)
            profile.DateOfBirth = request.DateOfBirth.Value;

        if (request.HeightCm.HasValue)
            profile.HeightCm = request.HeightCm.Value;

        if (request.BmiGoal.HasValue)
            profile.BmiGoal = request.BmiGoal.Value;

        if (!string.IsNullOrEmpty(request.ActivityLevel))
            profile.ActivityLevel = request.ActivityLevel;

        if (request.CurrentWeightKg.HasValue)
        {
            profile.CurrentWeightKg = request.CurrentWeightKg.Value;
            profile.LastWeightUpdate = DateTime.UtcNow;
            
            var weightEntry = new WeightHistory
            {
                UserId = userId,
                WeightKg = request.CurrentWeightKg.Value,
                RecordedAt = DateTime.UtcNow
            };
            await _unitOfWork.WeightHistories.AddAsync(weightEntry);
        }

        _unitOfWork.UserProfiles.Update(profile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile updated for user: {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToProfileResponse(profile));
    }

    #endregion

    #region Weight Tracking

    public async Task<Result<UserProfileResponse>> LogWeightAsync(int userId, LogWeightRequest request)
    {
        var profile = await _unitOfWork.UserProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            _logger.LogWarning("Log weight failed: Profile not found for {UserId}", userId);
            return Result<UserProfileResponse>.NotFound("Profile not found. Please create a profile first.");
        }

        // Create weight history entry
        var weightEntry = new WeightHistory
        {
            UserId = userId,
            WeightKg = request.WeightKg,
            RecordedAt = DateTime.UtcNow
        };
        await _unitOfWork.WeightHistories.AddAsync(weightEntry);

        // Update current weight in profile
        profile.CurrentWeightKg = request.WeightKg;
        profile.LastWeightUpdate = DateTime.UtcNow;
        _unitOfWork.UserProfiles.Update(profile);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Weight logged for user {UserId}: {Weight}kg", userId, request.WeightKg);
        return Result<UserProfileResponse>.Success(MapToProfileResponse(profile));
    }

    public async Task<Result<IEnumerable<WeightHistoryResponse>>> GetWeightHistoryAsync(int userId, int limit = 30)
    {
        var history = await _unitOfWork.WeightHistories.Query()
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.RecordedAt)
            .Take(limit)
            .ToListAsync();

        var responses = new List<WeightHistoryResponse>();
        WeightHistory? previous = null;

        // Process in chronological order for change calculation
        foreach (var entry in history.OrderBy(h => h.RecordedAt))
        {
            var response = new WeightHistoryResponse
            {
                Id = entry.Id,
                WeightKg = entry.WeightKg,
                RecordedAt = entry.RecordedAt,
                ChangeFromPrevious = previous != null ? entry.WeightKg - previous.WeightKg : null
            };
            responses.Add(response);
            previous = entry;
        }

        // Return in reverse chronological order (newest first)
        responses.Reverse();

        return Result<IEnumerable<WeightHistoryResponse>>.Success(responses);
    }

    #endregion

    #region Mappers

    private UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role?.Name ?? "unknown",
            IsActive = user.IsActive,
            HasProfile = user.UserProfile != null,
            CreatedAt = user.CreatedAt
        };
    }

    private UserProfileResponse MapToProfileResponse(UserProfile profile)
    {
        var age = _nutritionService.CalculateAge(profile.DateOfBirth);
        var bmi = _nutritionService.CalculateBmi(profile.HeightCm, profile.CurrentWeightKg);
        var bmr = _nutritionService.CalculateBmr(profile.Gender, profile.CurrentWeightKg, profile.HeightCm, age);
        var tdee = _nutritionService.CalculateTdee(bmr, profile.ActivityLevel);

        var weightGoal = _nutritionService.CalculateWeightFromBmi(profile.BmiGoal, profile.HeightCm);
        //maintain tolerance of 1kg
        var weightDifference = weightGoal - profile.CurrentWeightKg;
        var dietaryGoal = weightDifference < -1 ? "lose" : weightDifference > 1 ? "gain" : "maintain";
        var dailyCalories = _nutritionService.CalculateDailyCaloriesTarget(tdee, dietaryGoal);
        var (proteinG, carbsG, fatG) = _nutritionService.CalculateMacros(dailyCalories, dietaryGoal);

        return new UserProfileResponse
        {
            UserId = profile.UserId,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            Age = age,
            HeightCm = profile.HeightCm,
            CurrentWeightKg = profile.CurrentWeightKg,
            BmiGoal = profile.BmiGoal,
            WeightGoal = weightGoal,
            DietaryGoal = dietaryGoal,
            ActivityLevel = profile.ActivityLevel,
            Bmi = bmi,
            BmiCategory = _nutritionService.GetBmiCategory(bmi),
            Bmr = bmr,
            Tdee = tdee,
            DailyCaloriesTarget = dailyCalories,
            DailyProteinG = proteinG,
            DailyCarbsG = carbsG,
            DailyFatG = fatG,
            WeightToGoal = weightDifference,
            LastWeightUpdate = profile.LastWeightUpdate
        };
    }

    #endregion
}
