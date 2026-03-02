namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface IAFCDImportService
{
    Task<Result<AFCDImportResult>> ImportFromExcelAsync(string filePath);
}
