namespace DigitalMenuApi.Models.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required int RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    //navigation properties
    public Role Role { get; set; } = null!;
    public UserProfile? UserProfile { get; set; }  // Nullable - customers may not have profile
    public Restaurant? Restaurant { get; set; }  //Nullalble - only retaurant_admins have this 
    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
    public ICollection<WeightHistory> WeightHistories { get; set; } = new List<WeightHistory>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}