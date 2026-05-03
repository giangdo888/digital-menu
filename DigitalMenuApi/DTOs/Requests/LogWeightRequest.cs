namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class LogWeightRequest
{
    [Required]
    [Range(20, 500, ErrorMessage = "Weight must be between 20 and 500 kg")]
    public required decimal WeightKg { get; set; }

    public DateTime? RecordedAt { get; set; }
}
