using Microsoft.EntityFrameworkCore;
using RealEstateApp.Data;

namespace RealEstateApp.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        this.dbSet = _db.Set<T>();
    }

    public async Task AddAsync(T entity) => await dbSet.AddAsync(entity);
    
    public void Delete(T entity) => dbSet.Remove(entity);

    public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach(var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }
        return await query.ToListAsync();
    }

    public async Task<T?> GetAsync(int id) => await dbSet.FindAsync(id);
    
    public void Update(T entity) => dbSet.Update(entity);
}