namespace Mo_DataAccess.Repo;

public interface IGenericRepository<T> where T: class
{
     Task<IEnumerable<T>> GetAllAsync();
     Task<T> GetByIdAsync(long id);
     Task<T> CreateAsync(T item);
     Task<T> UpdateAsync(T item);
     Task<bool> DeleteAsync(long id);
}