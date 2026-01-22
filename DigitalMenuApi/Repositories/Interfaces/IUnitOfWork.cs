namespace DigitalMenuApi.Repositories.Interfaces;

using DigitalMenuApi.Models.Entities;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<UserProfile> UserProfiles { get; }
    IRepository<Restaurant> Restaurants { get; }
    IRepository<Category> Categories { get; }
    IRepository<Dish> Dishes { get; }
    IRepository<AFCDItem> AFCDItems { get; }
    IRepository<DishIngredient> DishIngredients { get; }
    IRepository<MealLog> MealLogs { get; }
    IRepository<WeightHistory> WeightHistories { get; }
    IRepository<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync();

    //explicit transaction management for complex operations
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}