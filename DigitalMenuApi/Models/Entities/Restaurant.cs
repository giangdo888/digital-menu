namespace DigitalMenuApi.Models.Entities;

public class Restaurant : BaseEntity
{
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }  // URL-friendly identifier (unique)
    public required string Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? OpeningHours { get; set; }  // e.g., "Mon-Fri 9am-10pm, Sat-Sun 10am-11pm"
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}