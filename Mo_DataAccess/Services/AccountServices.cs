using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class AccountServices :GenericRepository<Account>, IAccountServices
{
    public AccountServices(AppDbContext context) : base(context)
    {
    }
}