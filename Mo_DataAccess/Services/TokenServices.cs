namespace Mo_DataAccess.Services;

public class TokenServices:GenericRepository<Token>,ITokenServices
{
    public TokenServices(SwpGroup6Context context) : base(context)
    {
    }
}
