namespace Mo_DataAccess.Services;

public class RoleServices:GenericRepository<Role>    ,IRoleServices
{
    public RoleServices(AppDbContext context) : base(context)
    {
    }
}