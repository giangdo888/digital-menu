namespace DigitalMenuApi.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class UpdateDishRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [Range(0, 10000)]
    public decimal? Price { get; set; }

    [MaxLength(255)]
    [Url]
    public string? ImageUrl { get; set; }

    public int? DisplayOrder { get; set; }
}
