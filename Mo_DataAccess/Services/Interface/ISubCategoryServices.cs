using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface
{
    public interface ISubCategoryServices : IGenericRepository<SubCategory>
    {
        Task<SubCategory> AddAsync(SubCategory subCategory);
        Task<SubCategory> UpdateAsync(SubCategory subCategory);
        Task DeleteAsync(SubCategory subCategory);
        Task<IEnumerable<SubCategory>> GetSubCategoriesByCategoryIdAsync(long categoryId);
        Task<IEnumerable<SubCategory>> SearchSubCategoriesAsync(string? searchTerm);
        Task<bool> SubCategoryNameExistsAsync(string name, long categoryId, long? excludeId = null);
    }
}
