namespace DigitalMenuApi.DTOs.Responses;

public class UserProfileResponse
{
    // Basic info
    public int UserId { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Age { get; set; }

    // Measurements
    public decimal HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public decimal WeeklyWeightGoal { get; set; }
    public string DietaryGoal { get; set; } = string.Empty;
    public string ActivityLevel { get; set; } = string.Empty;

    // Calculated values
    public decimal Bmi { get; set; }
    public string BmiCategory { get; set; } = string.Empty;  // Underweight, Normal, Overweight, Obese
    public decimal Bmr { get; set; }  // Basal Metabolic Rate
    public decimal Tdee { get; set; }  // Total Daily Energy Expenditure

    // Daily targets based on goal
    public decimal DailyCaloriesTarget { get; set; }
    public decimal DailyProteinG { get; set; }
    public decimal DailyCarbsG { get; set; }
    public decimal DailyFatG { get; set; }

    // Progress
    public DateTime LastWeightUpdate { get; set; }
}
