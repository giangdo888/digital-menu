namespace DigitalMenuApi.Models.Entities;

public class UserProfile : BaseEntity
{
    public required int UserId { get; set; }
    public required string Gender { get; set; }  // "male" / "female"
    public required DateOnly DateOfBirth { get; set; }
    public required decimal HeightCm { get; set; }
    public required decimal CurrentWeightKg { get; set; }
    public required decimal GoalWeightKg { get; set; }
    public required string DietaryGoal { get; set; }  // "weight_loss" / "maintenance" / "muscle_gain"
    public DateTime LastWeightUpdate { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
