using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services
{
    // Triển khai service cho Category
    public class CategoryServices : GenericRepository<Category>, ICategoryServices
    {
        private readonly SwpGroup6Context _context;

        public CategoryServices(SwpGroup6Context context) : base(context)
        {
            _context = context;
        }
    }
}
