namespace DigitalMenuApi.Models.Entities;

public class Restaurant : BaseEntity
{
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    //navigation properties
    public User User { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();   
}