using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

    public interface IAccountServices:IGenericRepository<Account>
    {
        Task<Account?> ValidateLoginAsync(string identifier, string password);
        Task<(string accessToken, DateTime expiresAt, string refreshToken)> IssueTokensAsync(Account account, bool rememberMe);
        Task<Account> RegisterAsync(string username, string email, string password, string? phone);
        Task SendResetPasswordEmailAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
    }
