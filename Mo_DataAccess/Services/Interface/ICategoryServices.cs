using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface ICategoryServices:IGenericRepository<Category>
{
    Task<Category> AddAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task<IEnumerable<Category>> SearchCategoriesAsync(string? searchTerm);
}