namespace DigitalMenuApi.DTOs.Responses;

public class WeightHistoryResponse
{
    public int Id { get; set; }
    public decimal WeightKg { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal? ChangeFromPrevious { get; set; }  // Difference from last weight
}
