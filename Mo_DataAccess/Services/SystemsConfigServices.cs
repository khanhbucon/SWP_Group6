namespace Mo_DataAccess.Services;

public class SystemsConfigServices  :GenericRepository<SystemsConfig>,ISystemsConfigServices
{
    public SystemsConfigServices(SwpGroup6Context context) : base(context)
    {
    }
}
