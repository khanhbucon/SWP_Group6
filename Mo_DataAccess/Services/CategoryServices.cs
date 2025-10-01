using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class CategoryServices : GenericRepository<Category>, ICategoryServices
{
    public CategoryServices(AppDbContext context) : base(context)
    {
    }
}