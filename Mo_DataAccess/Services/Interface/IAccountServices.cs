using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

    public interface IAccountServices:IGenericRepository<Account>
    {
        Task<Account?> FindByUsernameOrEmailAsync(string identifier);
    }
