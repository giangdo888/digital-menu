namespace DigitalMenuApi.Repositories.Interfaces;

using DigitalMenuApi.Models.Entities;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);

    // ADVANCED - exposes IQueryable for complex queries
    IQueryable<T> Query();
}