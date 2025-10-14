using Mo_Entities;
using System.Linq;

namespace Mo_DataAccess
{
    public class ProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool SoftDelete(long id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id && (p.IsActive ?? false) == true);
            if (product != null)
            {
                product.IsActive = false;
                _context.SaveChanges();
            }

            return true;
        }
    }
}
