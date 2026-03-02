namespace DigitalMenuApi.Services.Implementations;

using DigitalMenuApi.DTOs.Requests;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AFCDItemService : IAFCDItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AFCDItemService> _logger;

    public AFCDItemService(IUnitOfWork unitOfWork, ILogger<AFCDItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<AFCDItemResponse>>> SearchAsync(string? query, int limit = 50)
    {
        var itemsQuery = _unitOfWork.AFCDItems.Query();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            itemsQuery = itemsQuery.Where(a =>
                a.Name.ToLower().Contains(searchTerm) ||
                (a.Variant != null && a.Variant.ToLower().Contains(searchTerm)));
        }

        var items = await itemsQuery
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Variant)
            .Take(limit)
            .ToListAsync();

        return Result<IEnumerable<AFCDItemResponse>>.Success(items.Select(MapToResponse));
    }

    public async Task<Result<AFCDItemResponse>> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.AFCDItems.GetByIdAsync(id);

        if (item == null)
        {
            return Result<AFCDItemResponse>.NotFound("AFCD item not found");
        }

        return Result<AFCDItemResponse>.Success(MapToResponse(item));
    }

    public async Task<Result<AFCDItemResponse>> CreateAsync(CreateAFCDItemRequest request)
    {
        var item = new AFCDItem
        {
            Name = request.Name,
            Variant = request.Variant,
            Calories = request.Calories,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG,
            FullNutritionJson = request.FullNutritionJson
        };

        await _unitOfWork.AFCDItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("AFCD item created: {ItemId} - {Name}", item.Id, item.Name);
        return Result<AFCDItemResponse>.Success(MapToResponse(item));
    }

    public async Task<Result<AFCDItemResponse>> UpdateAsync(int id, UpdateAFCDItemRequest request)
    {
        var item = await _unitOfWork.AFCDItems.GetByIdAsync(id);

        if (item == null)
        {
            return Result<AFCDItemResponse>.NotFound("AFCD item not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
            item.Name = request.Name;

        if (request.Variant != null)
            item.Variant = request.Variant;

        if (request.Calories.HasValue)
            item.Calories = request.Calories.Value;

        if (request.ProteinG.HasValue)
            item.ProteinG = request.ProteinG.Value;

        if (request.CarbsG.HasValue)
            item.CarbsG = request.CarbsG.Value;

        if (request.FatG.HasValue)
            item.FatG = request.FatG.Value;

        if (request.FullNutritionJson != null)
            item.FullNutritionJson = request.FullNutritionJson;

        _unitOfWork.AFCDItems.Update(item);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("AFCD item updated: {ItemId}", id);
        return Result<AFCDItemResponse>.Success(MapToResponse(item));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var item = await _unitOfWork.AFCDItems.Query()
            .Include(a => a.DishIngredients)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (item == null)
        {
            return Result.NotFound("AFCD item not found");
        }

        if (item.DishIngredients.Any())
        {
            return Result.Failure("Cannot delete AFCD item that is used in dishes");
        }

        _unitOfWork.AFCDItems.Delete(item);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("AFCD item deleted: {ItemId}", id);
        return Result.Success();
    }

    private static AFCDItemResponse MapToResponse(AFCDItem item)
    {
        return new AFCDItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Variant = item.Variant,
            Calories = item.Calories,
            ProteinG = item.ProteinG,
            CarbsG = item.CarbsG,
            FatG = item.FatG
        };
    }
}
