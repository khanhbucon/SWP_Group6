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

    public async Task<bool> CanDeleteCategoryAsync(long categoryId)
    {
        // Kiểm tra xem category có SubCategory nào không
        var hasSubCategories = await _context.SubCategories
            .AnyAsync(sc => sc.CategoryId == categoryId);

        if (hasSubCategories)
        {
            return false;
        }

        // Kiểm tra xem có Product nào thuộc về SubCategory của Category này không
        var hasProducts = await _context.Products
            .Include(p => p.SubCategory)
            .AnyAsync(p => p.SubCategory.CategoryId == categoryId);

        if (hasProducts)
        {
            return false;
        }

        return true;
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

        public async Task<bool> CategoryNameExistsAsync(string name, long? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
}