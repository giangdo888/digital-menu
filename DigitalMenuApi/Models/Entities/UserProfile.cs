namespace DigitalMenuApi.Models.Entities;

public class UserProfile : BaseEntity
{
    public required int UserId { get; set; }
    public required string Gender { get; set; }  // "male" / "female"
    public required DateOnly DateOfBirth { get; set; }
    public required decimal HeightCm { get; set; }
    public required decimal CurrentWeightKg { get; set; }
    public decimal BmiGoal { get; set; } = 20m; // 20 is the ideal BMI
    public string ActivityLevel { get; set; } = "sedentary";  // sedentary, lightly_active, moderately_active, very_active, extra_active
    public DateTime LastWeightUpdate { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
