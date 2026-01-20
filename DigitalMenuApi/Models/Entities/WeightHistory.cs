namespace DigitalMenuApi.Models.Entities;

public class WeightHistory : BaseEntity
{
    public required int UserId { get; set; }
    public required decimal WeightKg { get; set; }
    public DateTime RecordedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
