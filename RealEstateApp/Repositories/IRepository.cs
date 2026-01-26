using System.Linq.Expressions;

namespace RealEstateApp.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
    Task<T?> GetAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}