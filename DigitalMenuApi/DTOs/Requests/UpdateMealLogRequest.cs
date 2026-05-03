using System.Text.Json.Serialization;

namespace DigitalMenuApi.DTOs.Requests;

public class UpdateMealLogRequest
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int UserId { get; set; }

    public required int DishId { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
