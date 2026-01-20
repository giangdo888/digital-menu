namespace DigitalMenuApi.Models.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }

    //navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}