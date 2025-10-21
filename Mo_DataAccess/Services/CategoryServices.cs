using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Mo_DataAccess.Services;

public class CategoryServices : GenericRepository<Category>, ICategoryServices
{
    public CategoryServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<Category> AddAsync(Category category)
    {
        return await CreateAsync(category);
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        return await base.UpdateAsync(category);
    }

    public async Task DeleteAsync(Category category)
    {
        _dbSet.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> SearchCategoriesAsync(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return await GetAllAsync();
        }

        // Lấy tất cả categories và filter trong memory để tránh lỗi SQL
        var allCategories = await _dbSet.ToListAsync();
        
        return allCategories
            .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                      c.Id.ToString().Contains(searchTerm))
            .ToList();
    }
}