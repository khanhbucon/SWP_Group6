using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Mo_DataAccess.Services;

public class AccountServices :GenericRepository<Account>, IAccountServices
{
    public AccountServices(AppDbContext context) : base(context)
    {
    }

    public async Task<Account?> FindByUsernameOrEmailAsync(string identifier)
    {
        return await _context.Set<Account>()
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Username == identifier || a.Email == identifier);
    }
}