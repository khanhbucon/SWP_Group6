using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;

namespace Mo_Api.ApiController;
[Route("api/[controller]")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    private readonly IConfiguration _configuration;

    public AuthController(IAccountServices accountServices,  IConfiguration configuration)
    {
        _accountServices = accountServices;
       
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

        var account = await _accountServices.ValidateLoginAsync(request.Identifier, request.Password);
        if (account == null)
        {
            return Unauthorized("Tài khoản không tồn tại, bị khoá hoặc mật khẩu không đúng.");
        }

        var (accessToken, expires, refreshToken) = await _accountServices.IssueTokensAsync(account, request.RememberMe);

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expires,
            RefreshToken = refreshToken,
            Roles = account.Roles.Select(s => s.RoleName).ToList()

        });
    }

   
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var account = await _accountServices.RegisterAsync(request.Username, request.Email, request.Password, request.Phone);
            var response = new RegisterResponse
            {
                AccountId = account.Id,
                Username = account.Username,
                Email = account.Email,
                Roles = account.Roles.Select(r => r.RoleName).ToList()
            };
            return CreatedAtAction(nameof(Test), new { id = account.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
 }



