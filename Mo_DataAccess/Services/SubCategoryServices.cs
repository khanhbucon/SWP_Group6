namespace Mo_DataAccess.Services;

public class SubCategoryServices:GenericRepository<SubCategory>,ISubCategoryServices
{
    public SubCategoryServices(AppDbContext context) : base(context)
    {
    }
}