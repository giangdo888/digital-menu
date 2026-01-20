namespace DigitalMenuApi.Repositories.Interfaces;

using DigitalMenuApi.Models.Entities;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailExistsAsync(string email);
    Task<IEnumerable<User>> GetAllActiveAsync();
}