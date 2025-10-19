namespace Mo_DataAccess.Services;

public class SubCategoryServices:GenericRepository<SubCategory>,ISubCategoryServices
{
    public SubCategoryServices(SwpGroup6Context context) : base(context)
    {
    }
}