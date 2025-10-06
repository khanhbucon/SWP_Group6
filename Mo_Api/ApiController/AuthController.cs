using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Mo_Api.DTO.AccountDTO;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_Api.ApiController;
[Route("api/[controller]")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    private readonly ITokenServices _tokenServices;
    private readonly IConfiguration _configuration;

    public AuthController(IAccountServices accountServices, ITokenServices tokenServices, IConfiguration configuration)
    {
        _accountServices = accountServices;
        _tokenServices = tokenServices;
        _configuration = configuration;
    }


   [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Controller is working!");
    }



    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var account = await _accountServices.FindByUsernameOrEmailAsync(request.Identifier);
        if (account == null)
        {
            return Unauthorized("Tài khoản không tồn tại hoặc mật khẩu không đúng.");
        }

        if (account.IsActive.HasValue && !account.IsActive.Value)
        {
            return Unauthorized("Tài khoản đã bị vô hiệu hoá.");
        }

        // Hỗ trợ kiểm tra mật khẩu dạng plaintext hoặc SHA256 (tương thích tạm thời)
        var isPasswordValid = account.Password == request.Password || account.Password == ComputeSha256(request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Tài khoản không tồn tại hoặc mật khẩu không đúng.");
        }

        var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(500, "Thiếu cấu hình Jwt:Key");
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

        // Refresh token
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpires = request.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7);

        await _tokenServices.CreateAsync(new Token
        {
            AccountId = account.Id,
            AccessToken = ComputeSha256(accessToken),
            RefreshToken = refreshToken,
            ExpiresAt = refreshExpires,
            CreatedAt = DateTime.UtcNow
        });

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expires,
            RefreshToken = refreshToken
        });
    }

    private static string ComputeSha256(string raw)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
 }



