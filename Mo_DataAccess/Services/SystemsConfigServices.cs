namespace Mo_DataAccess.Services;

public class SystemsConfigServices  :GenericRepository<SystemsConfig>,ISystemsConfigServices
{
    public SystemsConfigServices(AppDbContext context) : base(context)
    {
    }
}