namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateProfileRequest
{
    [RegularExpression("^(male|female)$", ErrorMessage = "Gender must be 'male' or 'female'")]
    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Range(50, 300, ErrorMessage = "Height must be between 50 and 300 cm")]
    public decimal? HeightCm { get; set; }

    [Range(18.5, 24.9, ErrorMessage = "Healthy BMI goal must be between 18.5 and 24.9")]
    public decimal? BmiGoal { get; set; } = 20m; // 20 is the ideal Bmi

    // Note: CurrentWeightKg is NOT here - weight is updated via LogWeight endpoint
}
