namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IAFCDItemService
{
    // Search (restaurant_admin + system_admin)
    Task<Result<IEnumerable<AFCDItemResponse>>> SearchAsync(string? query, int limit = 50);
    Task<Result<AFCDItemResponse>> GetByIdAsync(int id);

    // CRUD (system_admin only)
    Task<Result<AFCDItemResponse>> CreateAsync(CreateAFCDItemRequest request);
    Task<Result<AFCDItemResponse>> UpdateAsync(int id, UpdateAFCDItemRequest request);
    Task<Result> DeleteAsync(int id);
}
