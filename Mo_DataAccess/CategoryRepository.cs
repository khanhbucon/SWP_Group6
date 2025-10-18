using Mo_Entities.Models;
using System.Collections.Generic;
using System.Linq;

namespace Mo_DataAccess
{
    public class CategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ danh mục
        public IEnumerable<Category> GetAllCategories()
        {
            return _context.Categories.ToList();
        }
    }
}
