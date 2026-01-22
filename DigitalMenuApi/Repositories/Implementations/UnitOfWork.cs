using DigitalMenuApi.Data;
using DigitalMenuApi.Models.Entities;
using DigitalMenuApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DigitalMenuApi.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    //Lazy initialization of repositories
    private IRepository<User>? _users;
    private IRepository<Role>? _roles;
    private IRepository<UserProfile>? _userProfiles;
    private IRepository<Restaurant>? _restaurants;
    private IRepository<Category>? _categories;
    private IRepository<Dish>? _dishes;
    private IRepository<AFCDItem>? _afcdItems;
    private IRepository<DishIngredient>? _dishIngredients;
    private IRepository<MealLog>? _mealLogs;
    private IRepository<WeightHistory>? _weightHistories;
    private IRepository<RefreshToken>? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    //Lazy pattern - create repo only on first access
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
    public IRepository<UserProfile> UserProfiles => _userProfiles ??= new Repository<UserProfile>(_context);
    public IRepository<Restaurant> Restaurants => _restaurants ??= new Repository<Restaurant>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Dish> Dishes => _dishes ??= new Repository<Dish>(_context);
    public IRepository<AFCDItem> AFCDItems => _afcdItems ??= new Repository<AFCDItem>(_context);
    public IRepository<DishIngredient> DishIngredients => _dishIngredients ??= new Repository<DishIngredient>(_context);
    public IRepository<MealLog> MealLogs => _mealLogs ??= new Repository<MealLog>(_context);
    public IRepository<WeightHistory> WeightHistories => _weightHistories ??= new Repository<WeightHistory>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null) {
            await _transaction.CommitAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null) {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}