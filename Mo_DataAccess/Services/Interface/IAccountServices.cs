using Microsoft.Identity.Client;
using Mo_DataAccess.Repo;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;
using System.Net.Sockets;

namespace Mo_DataAccess.Services.Interface;

    public interface IAccountServices:IGenericRepository<Account>
    {
        Task<Account?> ValidateLoginAsync(string identifier, string password);
        Task<(string accessToken, DateTime expiresAt, string refreshToken)> IssueTokensAsync(Account account, bool rememberMe);
        Task<Account> RegisterAsync(string username, string email, string password, string? phone);
        Task SendResetPasswordEmailAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);


    Task<Account> AdminCreateAccountAsync(string username, string email, string phone, string password, long roleId, bool isActive = true);

    Task<List<ListAccountResponse>> GetAllAccountAsync();

    Task<ListAccountResponse> GetAccountsByIdAsync(long id);

    Task<Account> GrantSellerRoleAsync(long accountId);
    Task<Account> BanUserAsync(long accountId);
    Task<Mo_Entities.ModelResponse.ProfileResponse> GetProfileByIdAsync(long userId);




}
