namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class CreateProfileRequest
{
    [Required]
    [RegularExpression("^(male|female)$", ErrorMessage = "Gender must be 'male' or 'female'")]
    public required string Gender { get; set; }

    [Required]
    public required DateOnly DateOfBirth { get; set; }

    [Required]
    [Range(50, 300, ErrorMessage = "Height must be between 50 and 300 cm")]
    public required decimal HeightCm { get; set; }

    [Required]
    [Range(20, 200, ErrorMessage = "Weight must be between 20 and 200 kg")]
    public required decimal CurrentWeightKg { get; set; }

    [Range(18.5, 24.9, ErrorMessage = "HealthyBMI goal must be between 18.5 and 24.9")]
    public decimal BmiGoal { get; set; } = 20m;
}
