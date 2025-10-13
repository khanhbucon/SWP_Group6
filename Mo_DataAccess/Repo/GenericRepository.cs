using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;

namespace Mo_DataAccess.Repo;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{   
    protected readonly SwpGroup6Context _context;
    protected readonly DbSet<T> _dbSet;
    public GenericRepository(SwpGroup6Context context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> CreateAsync(T item)
    {
            _dbSet.Add(item);
            await _context.SaveChangesAsync();
            return item;
    }

    public async Task<T> UpdateAsync(T item)
    {
        _dbSet.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity =await _dbSet.FindAsync(id);
        if (entity == null)
        {
            return false;
        }
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
