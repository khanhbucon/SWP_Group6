using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Mo_DataAccess.Services
{
    public class SubCategoryServices : ISubCategoryServices
    {
        private readonly SwpGroup6Context _context;

        public SubCategoryServices(SwpGroup6Context context)
        {
            _context = context;
        }

        public List<SubCategory> GetAll()
        {
            return _context.SubCategories
                           .Include(x => x.Category)
                           .OrderBy(x => x.Id)
                           .ToList();
        }

        public SubCategory? GetById(long id)
        {
            return _context.SubCategories
                           .Include(x => x.Category)
                           .FirstOrDefault(x => x.Id == id);
        }

        public void Add(SubCategory subCategory)
        {
            _context.SubCategories.Add(subCategory);
            _context.SaveChanges();
        }

        public void Update(SubCategory subCategory)
        {
            var existing = _context.SubCategories.FirstOrDefault(x => x.Id == subCategory.Id);
            if (existing != null)
            {
                existing.Name = subCategory.Name;
                existing.CategoryId = subCategory.CategoryId;
                existing.IsActive = subCategory.IsActive;
                _context.SaveChanges();
            }
        }

        public void Delete(long id)
        {
            var sub = _context.SubCategories.FirstOrDefault(x => x.Id == id);
            if (sub != null)
            {
                _context.SubCategories.Remove(sub);
                _context.SaveChanges();
            }
        }
    }
}
