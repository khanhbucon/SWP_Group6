using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class AccountServices :GenericRepository<Account>, IAccountServices
{
    private readonly IConfiguration _configuration;

    public AccountServices(AppDbContext context, IConfiguration configuration) : base(context)
    {
        _configuration = configuration;
    }

   

    public async Task<Account> RegisterAsync(string username, string email, string password, string? phone)
    {
        // unique checks
        var usernameExists = await _context.Set<Account>().AnyAsync(a => a.Username == username);
        if (usernameExists) throw new InvalidOperationException("Tên đăng nhập đã tồn tại");

        var emailExists = await _context.Set<Account>().AnyAsync(a => a.Email == email);
        if (emailExists) throw new InvalidOperationException("Email đã tồn tại");

        var account = new Account
        {
            Username = username,
            Email = email,
            Password = ComputeSha256(password),
            Phone = phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync();

        // Assign default Buyer role id=3 if exists
        var buyerRole = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Id == 3);
        if (buyerRole != null)
        {
            account.Roles.Add(buyerRole);
            _context.Update(account);
            await _context.SaveChangesAsync();
        }

        return account;
    }

   

    public async Task<(string accessToken, DateTime expiresAt, string refreshToken)> IssueTokensAsync(Account account, bool rememberMe)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Missing Jwt:Key configuration");
        }
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var durationInMinutes = int.TryParse(_configuration["Jwt:DurationInMinutes"], out var mins) ? mins : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(durationInMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username),
            new Claim(JwtRegisteredClaimNames.Email, account.Email)
        };
        foreach (var role in account.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7);

        _context.Set<Token>().Add(new Token
        {
            AccountId = account.Id,
            AccessToken = ComputeSha256(accessToken),
            RefreshToken = refreshToken,
            ExpiresAt = refreshExpires,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return (accessToken, expires, refreshToken);
    }

    public async Task SendResetPasswordEmailAsync(string email)
    {
        var account = await _context.Set<Account>().FirstOrDefaultAsync(a => a.Email == email);
        if (account == null)
        {
            return;
        }

        var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Missing Jwt:Key configuration");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(15);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim("purpose", "reset")
        };
        var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: creds);
        var resetToken = new JwtSecurityTokenHandler().WriteToken(token);

        var resetBaseUrl = _configuration["App:ResetPasswordUrl"] ?? string.Empty;
        var resetLink = string.IsNullOrWhiteSpace(resetBaseUrl)
            ? $"https://example.com/reset?token={WebUtility.UrlEncode(resetToken)}"
            : $"{resetBaseUrl}{WebUtility.UrlEncode(resetToken)}";

        // Read sender config from SystemsConfig in DB
        var sysCfg = await _context.Set<SystemsConfig>().AsNoTracking().FirstOrDefaultAsync();
        if (sysCfg == null || string.IsNullOrWhiteSpace(sysCfg.Email) || string.IsNullOrWhiteSpace(sysCfg.GoogleAppPassword))
        {
            throw new InvalidOperationException("Thiếu cấu hình email trong SystemsConfig");
        }

        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(sysCfg.Email, sysCfg.GoogleAppPassword)
        };
        var mail = new MailMessage
        {
            From = new MailAddress(sysCfg.Email),
            Subject = "Đặt lại mật khẩu",
            Body = $"Nhấn vào liên kết để đặt lại mật khẩu: {resetLink}",
            IsBodyHtml = false
        };
        mail.To.Add(account.Email);
        await smtp.SendMailAsync(mail);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Missing Jwt:Key configuration");
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
        var purpose = principal.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
        if (purpose != "reset") throw new SecurityTokenException("Invalid purpose");

        var sub = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (!long.TryParse(sub, out var accountId)) throw new SecurityTokenException("Invalid subject");

        var account = await _context.Set<Account>().FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return;

        account.Password = ComputeSha256(newPassword);
        _context.Update(account);
        await _context.SaveChangesAsync();
    }

    private static string ComputeSha256(string raw)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public async Task<Account?> FindByUsernameOrEmailAsync(string identifier)
    {
        return await _context.Set<Account>()
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Username == identifier || a.Email == identifier);
    }

    public async Task<Account?> ValidateLoginAsync(string identifier, string password)
    {
        var account = await FindByUsernameOrEmailAsync(identifier);
        if (account == null)
        {
            return null;
        }
        if (account.IsActive.HasValue && !account.IsActive.Value)
        {
            return null;
        }
        var hashed = ComputeSha256(password);
        var ok = account.Password == password || account.Password == hashed;
        return ok ? account : null;
    }
}