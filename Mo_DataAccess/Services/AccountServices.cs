using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class AccountServices :GenericRepository<Account>, IAccountServices
{
    private readonly IConfiguration _configuration;

    public AccountServices(SwpGroup6Context context, IConfiguration configuration) : base(context)
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

    public async Task<Account> AdminCreateAccountAsync(string username, string email, string phone, string password, long roleId, bool isActive = true)
    {
        var usernameExists = await _context.Set<Account>().AnyAsync(a => a.Username == username);
        if (usernameExists) throw new InvalidOperationException("Tên đăng nhập đã tồn tại");

        var emailExists = await _context.Set<Account>().AnyAsync(a => a.Email == email);
        if (emailExists) throw new InvalidOperationException("Email đã tồn tại");

        if (!string.IsNullOrEmpty(phone))
        {
            var phoneExists = await _context.Set<Account>().AnyAsync(a => a.Phone == phone);
            if (phoneExists) throw new InvalidOperationException("Số điện thoại đã tồn tại");
        }
        // Lấy role trước
        var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) throw new InvalidOperationException("Vai trò không tồn tại");



        var newaccount = new Account
        {
            Username = username,
            Email = email,
            Password = ComputeSha256(password),
            Phone = phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = isActive,
            Roles = new List<Role> { role }
        };

        // Sử dụng _context trực tiếp để EF tracking đúng
        _context.Set<Account>().Add(newaccount);
        await _context.SaveChangesAsync();
        return newaccount;
    }

    public async Task<List<ListAccountResponse>> GetAllAccountAsync()
    {
        var accounts = await _context.Accounts
       .Include(a => a.Roles)
       .Include(a => a.Shops)
       .ThenInclude(s => s.Products) 
       .ToListAsync();

        var responseList = new List<ListAccountResponse>();

        foreach (var user in accounts)
        {
            //  Đếm số đơn hàng đã mua
            var totalOrders = await _context.Set<OrderProduct>()
                .CountAsync(o => o.AccountId == user.Id);

            //  Đếm số gian hàng
            var totalShops = user.Shops?.Count ?? 0;

            // ✅ Đếm số sản phẩm đã bán (từ Products của Shop)
            var totalProductsSold = 0;
            if (user.Shops != null && user.Shops.Any())
            {
                // Lấy tất cả ProductId từ các Shop của user
                var productIds = user.Shops
                    .SelectMany(s => s.Products)
                    .Select(p => p.Id)
                    .ToList();

                // Đếm tổng quantity từ các OrderProduct có ProductVariant thuộc các Product này
                totalProductsSold = await _context.Set<OrderProduct>()
                    .Where(o => _context.Set<ProductVariant>()
                        .Where(pv => productIds.Contains(pv.ProductId))
                        .Select(pv => pv.Id)
                        .Contains(o.ProductVariantId))
                    .SumAsync(o => o.Quantity);
            }

            var response = new ListAccountResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Balance = user.Balance,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive,
                Roles = user.Roles.Select(r => r.RoleName).ToList(),

                CreatedAt = user.CreatedAt,
                TotalOrders = totalOrders,
                TotalShops = totalShops,
                TotalProductsSold = totalProductsSold,
                IsEKYCVerified = !string.IsNullOrEmpty(user.IdentificationF) && !string.IsNullOrEmpty(user.IdentificationB)
            };

            responseList.Add(response);
        }

        return responseList;

    }

    public async Task<ListAccountResponse> GetAccountsByIdAsync(long id)
    {

        var account = await _context.Accounts
          .Include(a => a.Roles)
          .Include(a => a.Shops)
          .ThenInclude(s => s.Products)
          .SingleOrDefaultAsync(a => a.Id == id);

        if (account == null)
            throw new InvalidOperationException($"Account với ID {id} không tồn tại");

        //  Đếm số đơn hàng đã mua
        var totalOrders = await _context.Set<OrderProduct>()
            .CountAsync(o => o.AccountId == account.Id);

        //  Đếm số gian hàng
        var totalShops = account.Shops?.Count ?? 0;

        //  Đếm số sản phẩm đã bán
        var totalProductsSold = 0;
        if (account.Shops != null && account.Shops.Any())
        {
            var productIds = account.Shops
                .SelectMany(s => s.Products)
                .Select(p => p.Id)
                .ToList();

            totalProductsSold = await _context.Set<OrderProduct>()
                .Where(o => _context.Set<ProductVariant>()
                    .Where(pv => productIds.Contains(pv.ProductId))
                    .Select(pv => pv.Id)
                    .Contains(o.ProductVariantId))
                .SumAsync(o => o.Quantity);
        }

        //  Thêm thông tin chi tiết về Shop (nếu cần)
        var shopDetails = account.Shops?.Select(s => new
        {
            ShopId = s.Id,
            ShopName = s.Name,
            ProductCount = s.Products?.Count ?? 0,
            IsActive = s.IsActive
        }).ToList();

        return new ListAccountResponse
        {
            UserId = account.Id,
            Username = account.Username,
            Balance = account.Balance,
            Email = account.Email,
            Phone = account.Phone,
            IsActive = account.IsActive,
            Roles = account.Roles.Select(r => r.RoleName).ToList(),

            CreatedAt = account.CreatedAt,
            TotalOrders = totalOrders,
            TotalShops = totalShops,
            TotalProductsSold = totalProductsSold,
            IsEKYCVerified = !string.IsNullOrEmpty(account.IdentificationF) && !string.IsNullOrEmpty(account.IdentificationB)
        };
    }

    public async Task<ProfileResponse> GetProfileByIdAsync(long userId)
    {
        var account = await _context.Accounts
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Id == userId);

        if (account == null)
            throw new InvalidOperationException("User not found");

        // Tính toán thống kê
        var totalOrders = await _context.OrderProducts
            .Where(op => op.AccountId == userId)
            .CountAsync();

        var totalShops = await _context.Shops
            .Where(s => s.AccountId == userId)
            .CountAsync();

        var totalProductsSold = await _context.OrderProducts
            .Where(op => op.AccountId == userId)
            .SumAsync(op => op.Quantity);

        return new Mo_Entities.ModelResponse.ProfileResponse
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            Phone = account.Phone,
            Balance = account.Balance,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            Roles = account.Roles.Select(r => r.RoleName).ToList(),
            IsEKYCVerified = !string.IsNullOrEmpty(account.IdentificationF) && !string.IsNullOrEmpty(account.IdentificationB),
            TotalOrders = totalOrders,
            TotalShops = totalShops,
            TotalProductsSold = totalProductsSold,
            //IdentificationF = account.IdentificationF,  // Thêm dòng này
            //IdentificationB = account.IdentificationB    // Thêm dòng này
        };
    }

    public async Task<Account> UpdateProfileAsync(long userId, UpdateProfileRequest request)
    {
        var account = await _context.Set<Account>().FirstOrDefaultAsync(a => a.Id == userId);
        if (account == null)
            throw new InvalidOperationException("Tài khoản không tồn tại");

        // Kiểm tra username đã tồn tại chưa (trừ chính user hiện tại)
        if (await _context.Set<Account>().AnyAsync(a => a.Username == request.Username && a.Id != userId))
            throw new InvalidOperationException("Username đã tồn tại");

        // Kiểm tra email đã tồn tại chưa (trừ chính user hiện tại)
        if (await _context.Set<Account>().AnyAsync(a => a.Email == request.Email && a.Id != userId))
            throw new InvalidOperationException("Email đã tồn tại");

        // Kiểm tra phone đã tồn tại chưa (nếu có phone)
        if (!string.IsNullOrEmpty(request.Phone))
        {
            if (await _context.Set<Account>().AnyAsync(a => a.Phone == request.Phone && a.Id != userId))
                throw new InvalidOperationException("Số điện thoại đã tồn tại");
        }

        // Cập nhật thông tin
        account.Username = request.Username;
        account.Email = request.Email;
        account.Phone = request.Phone;
        account.IdentificationF = request.IdentificationF;
        account.IdentificationB = request.IdentificationB;
        account.UpdatedAt = DateTime.UtcNow;

        return await UpdateAsync(account);
    }

    // Method riêng để update KYC
    public async Task<Account> UpdateKYCAsync(long userId, string identificationF, string identificationB)
    {
        var account = await _context.Set<Account>().FirstOrDefaultAsync(a => a.Id == userId);
        if (account == null)
            throw new InvalidOperationException("Tài khoản không tồn tại");

        account.IdentificationF = identificationF;
        account.IdentificationB = identificationB;
        account.UpdatedAt = DateTime.UtcNow;

        return await UpdateAsync(account);
    }

    public async Task<Account> GrantSellerRoleAsync(long accountId)
    {
        var account = await _context.Set<Account>()
        .Include(a => a.Roles)
        .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
            throw new InvalidOperationException("Tài khoản không tồn tại");

        // Kiểm tra user đã có Seller role chưa
        var hasSellerRole = account.Roles.Any(r => r.RoleName == "Seller");

        if (hasSellerRole)
            throw new InvalidOperationException("User đã có quyền Seller");

        // Lấy Seller role
        var sellerRole = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.RoleName == "Seller");

        if (sellerRole == null)
            throw new InvalidOperationException("Không tìm thấy role Seller");

        account.Roles.Add(sellerRole);
        account.UpdatedAt = DateTime.UtcNow;

        return await UpdateAsync(account);
    }

    public async Task<Account> BanUserAsync(long accountId)
    {

        var account = await _context.Set<Account>()
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
            throw new InvalidOperationException("Tài khoản không tồn tại");

        account.IsActive = !account.IsActive;
        account.UpdatedAt = DateTime.UtcNow;
        return await UpdateAsync(account);
    }

    public async Task<Account> ToggleBanUserAsync(long accountId)
    {
        var account = await GetByIdAsync(accountId);

        if (account == null)
            throw new InvalidOperationException("Tài khoản không tồn tại");

        account.IsActive = !account.IsActive;
        account.UpdatedAt = DateTime.UtcNow;
        return await UpdateAsync(account);
    }
}
