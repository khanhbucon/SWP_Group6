namespace Mo_DataAccess.Services;

public class TokenServices:GenericRepository<Token>,ITokenServices
{
    public TokenServices(AppDbContext context) : base(context)
    {
    }
}