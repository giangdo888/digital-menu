namespace DigitalMenuApi.DTOs.Responses;

public class AFCDImportResult
{
    public int TotalRows { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }  // Already exists (by PublicFoodKey)
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
