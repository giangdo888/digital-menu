namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IUserService
{
    // User operations
    Task<Result<UserResponse>> GetCurrentUserAsync(int userId);
    Task<Result<UserResponse>> GetUserByIdAsync(int userId);
    Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync();
    Task<Result<UserResponse>> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task<Result> DeactivateUserAsync(int userId);
    Task<Result> ActivateUserAsync(int userId);

    // Profile operations
    Task<Result<UserProfileResponse>> GetProfileAsync(int userId);
    Task<Result<UserProfileResponse>> CreateProfileAsync(int userId, CreateProfileRequest request);
    Task<Result<UserProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    // Weight tracking
    Task<Result<UserProfileResponse>> LogWeightAsync(int userId, LogWeightRequest request);
    Task<Result<IEnumerable<WeightHistoryResponse>>> GetWeightHistoryAsync(int userId, int limit = 30);
}
