namespace DigitalMenuApi.Services.Implementations;

using ClosedXML.Excel;
using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using DigitalMenuApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class AFCDImportService : IAFCDImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AFCDImportService> _logger;

    // Column indices (1-based for ClosedXML)
    private const int ColPublicFoodKey = 1;  // A
    private const int ColFoodName = 4;       // D
    private const int ColEnergyKJ = 5;       // E
    private const int ColProtein = 8;        // H
    private const int ColFat = 10;           // J
    private const int ColCarbs = 36;         // AJ

    private const decimal KJtoKcalFactor = 4.184m;
    private const int BatchSize = 100;

    public AFCDImportService(IUnitOfWork unitOfWork, ILogger<AFCDImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AFCDImportResult>> ImportFromExcelAsync(string filePath)
    {
        var result = new AFCDImportResult();

        if (!File.Exists(filePath))
        {
            return Result<AFCDImportResult>.Failure($"File not found: {filePath}");
        }

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet("All solids & liquids per 100 g");

            if (worksheet == null)
            {
                return Result<AFCDImportResult>.Failure("Worksheet 'All solids & liquids per 100 g' not found");
            }

            // Get all existing PublicFoodKeys for deduplication
            var existingKeys = await _unitOfWork.AFCDItems.Query()
                .Where(a => a.PublicFoodKey != null)
                .Select(a => a.PublicFoodKey!)
                .ToListAsync();
            var existingKeysSet = existingKeys.ToHashSet();

            // Get header row for JSON field names
            var headerRow = worksheet.Row(1);
            var headers = new List<string>();
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 272;
            for (int col = 1; col <= lastColumn; col++)
            {
                headers.Add(headerRow.Cell(col).GetString());
            }

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            result.TotalRows = lastRow - 1; // Exclude header

            var batch = new List<AFCDItem>();

            // Process data rows (skip header)
            for (int rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                try
                {
                    var row = worksheet.Row(rowNum);
                    var publicFoodKey = row.Cell(ColPublicFoodKey).GetString()?.Trim();

                    // Skip if already exists
                    if (!string.IsNullOrEmpty(publicFoodKey) && existingKeysSet.Contains(publicFoodKey))
                    {
                        result.Skipped++;
                        continue;
                    }

                    // Parse Food Name into Name and Variant
                    var foodName = row.Cell(ColFoodName).GetString()?.Trim() ?? "";
                    var (name, variant) = SplitFoodName(foodName);

                    if (string.IsNullOrEmpty(name))
                    {
                        result.Failed++;
                        result.Errors.Add($"Row {rowNum}: Empty food name");
                        continue;
                    }

                    // Parse nutrition values
                    var energyKJ = ParseDecimal(row.Cell(ColEnergyKJ));
                    var protein = ParseDecimal(row.Cell(ColProtein));
                    var fat = ParseDecimal(row.Cell(ColFat));
                    var carbs = ParseDecimal(row.Cell(ColCarbs));

                    // Convert kJ to kcal
                    var calories = energyKJ / KJtoKcalFactor;

                    // Build full nutrition JSON
                    var fullNutrition = new Dictionary<string, object?>();
                    for (int col = 1; col <= lastColumn; col++)
                    {
                        var headerName = headers[col - 1];
                        if (!string.IsNullOrEmpty(headerName))
                        {
                            var cellValue = row.Cell(col).Value;
                            fullNutrition[headerName] = cellValue.IsBlank ? null : cellValue.ToString();
                        }
                    }

                    var item = new AFCDItem
                    {
                        PublicFoodKey = publicFoodKey,
                        Name = name,
                        Variant = variant,
                        Calories = Math.Round(calories, 2),
                        ProteinG = Math.Round(protein, 2),
                        FatG = Math.Round(fat, 2),
                        CarbsG = Math.Round(carbs, 2),
                        FullNutritionJson = JsonSerializer.Serialize(fullNutrition)
                    };

                    batch.Add(item);
                    existingKeysSet.Add(publicFoodKey ?? ""); // Prevent duplicates within file

                    // Save batch
                    if (batch.Count >= BatchSize)
                    {
                        await SaveBatchAsync(batch);
                        result.Imported += batch.Count;
                        _logger.LogInformation("Imported {Count} items (total: {Total})", batch.Count, result.Imported);
                        batch.Clear();
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {rowNum}: {ex.Message}");
                }
            }

            // Save remaining items
            if (batch.Count > 0)
            {
                await SaveBatchAsync(batch);
                result.Imported += batch.Count;
            }

            _logger.LogInformation("AFCD import completed: {Imported} imported, {Skipped} skipped, {Failed} failed",
                result.Imported, result.Skipped, result.Failed);

            return Result<AFCDImportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing AFCD data from {FilePath}", filePath);
            return Result<AFCDImportResult>.Failure($"Import failed: {ex.Message}");
        }
    }

    private static (string name, string? variant) SplitFoodName(string foodName)
    {
        if (string.IsNullOrEmpty(foodName))
            return ("", null);

        var firstCommaIndex = foodName.IndexOf(',');
        if (firstCommaIndex == -1)
            return (foodName, null);

        var name = foodName[..firstCommaIndex].Trim();
        var variant = foodName[(firstCommaIndex + 1)..].Trim();

        return (name, string.IsNullOrEmpty(variant) ? null : variant);
    }

    private static decimal ParseDecimal(IXLCell cell)
    {
        if (cell.Value.IsBlank)
            return 0;

        if (cell.Value.IsNumber)
            return (decimal)cell.Value.GetNumber();

        var stringValue = cell.GetString()?.Trim();
        if (string.IsNullOrEmpty(stringValue))
            return 0;

        // Handle values like "N/A", "Tr", etc.
        if (decimal.TryParse(stringValue, out var result))
            return result;

        return 0;
    }

    private async Task SaveBatchAsync(List<AFCDItem> items)
    {
        foreach (var item in items)
        {
            await _unitOfWork.AFCDItems.AddAsync(item);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
