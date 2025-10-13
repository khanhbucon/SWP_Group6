namespace Mo_DataAccess.Services;

public class RoleServices:GenericRepository<Role>    ,IRoleServices
{
    public RoleServices(SwpGroup6Context context) : base(context)
    {
    }
}