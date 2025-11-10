
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Mo_DataAccess.Services
{
    public class SubCategoryServices : GenericRepository<SubCategory>, ISubCategoryServices
    {
        public SubCategoryServices(SwpGroup6Context context) : base(context)
        {
        }

        public async Task<SubCategory> AddAsync(SubCategory subCategory)
        {
            return await CreateAsync(subCategory);
        }

        public async Task<SubCategory> UpdateAsync(SubCategory subCategory)
        {
            return await base.UpdateAsync(subCategory);
        }

        public async Task DeleteAsync(SubCategory subCategory)
        {
            _dbSet.Remove(subCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SubCategory>> GetSubCategoriesByCategoryIdAsync(long categoryId)
        {
            return await _dbSet
                .Where(sc => sc.CategoryId == categoryId)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubCategory>> SearchSubCategoriesAsync(string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await GetAllAsync();
            }

            // Lấy tất cả subcategories và filter trong memory để tránh lỗi SQL
            var allSubCategories = await _dbSet
                .Include(sc => sc.Category)
                .ToListAsync();
            
            return allSubCategories
                .Where(sc => sc.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                          sc.Id.ToString().Contains(searchTerm) ||
                          sc.Category.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<bool> SubCategoryNameExistsAsync(string name, long categoryId, long? excludeId = null)
        {
            var query = _dbSet.Where(sc => sc.Name.ToLower() == name.ToLower() && sc.CategoryId == categoryId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(sc => sc.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();

        }
    }
}
