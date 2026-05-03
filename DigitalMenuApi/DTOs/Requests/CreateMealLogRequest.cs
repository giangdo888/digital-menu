using System.Text.Json.Serialization;

namespace DigitalMenuApi.DTOs.Requests;

public class CreateMealLogRequest
{
    [JsonIgnore]
    public int UserId { get; set; }
    public required int DishId { get; set; }
    public DateTime? ConsumedAt { get; set; }
}