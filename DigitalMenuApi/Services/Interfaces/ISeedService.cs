namespace DigitalMenuApi.Services.Interfaces;

using DigitalMenuApi.DTOs.Responses;
using DigitalMenuApi.Helpers;

public interface ISeedService
{
    Task<Result<SeedResult>> SeedSampleDataAsync();
}
