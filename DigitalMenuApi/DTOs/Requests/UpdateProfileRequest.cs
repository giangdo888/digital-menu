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
    public decimal? BmiGoal { get; set; }

    [Range(20, 200, ErrorMessage = "Weight must be between 20 and 200 kg")]
    public decimal? CurrentWeightKg { get; set; }

    [RegularExpression("^(sedentary|lightly_active|moderately_active|very_active|extra_active)$", ErrorMessage = "Activity level must be a valid option")]
    public string? ActivityLevel { get; set; }
}
