using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Mo_Api.Extensions;
namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    public AccountController(IAccountServices accountServices)
    {
        _accountServices = accountServices;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        await _accountServices.SendResetPasswordEmailAsync(request.Email);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        await _accountServices.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok();
    }

    [HttpGet("Admin/GetAllAccount")]
    [Authorize(Roles ="Admin")]
    public async Task<IActionResult> GetAllUser()
    {
        var user = await _accountServices.GetAllAccountAsync();

        if (user == null || user.Count == 0)
        {
            return NotFound(new { Message = "No users found." });
        }

        return Ok(user);
    }
    [HttpGet("{id}")]
     [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccountById(long id)
    {
        try
        {
            var account = await _accountServices.GetAccountsByIdAsync(id);
            return Ok(new { Success = true, Data = account });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thông tin tài khoản" });
        }
    }

    [HttpPost("admin/create-account")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminCreateAccount([FromBody] AdminCreateAccountRequest request)
    {
        try
        {
            var account = await _accountServices.AdminCreateAccountAsync(
                request.Username,
                request.Email,
                request.Phone,
                request.Password,
                request.RoleId,
                request.IsActive
            );

            return Ok(new { Success = true, Message = "Tạo tài khoản thành công", AccountId = account.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo tài khoản" });
        }
    }

    [HttpPost("admin/{id}/banUser")]
     [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleBanUser(long id)
    {
        try
        {
            var account = await _accountServices.BanUserAsync(id);

            var response = new
            {
                Id = account.Id,
                Username = account.Username,
                IsActive = account.IsActive,
                UpdatedAt = account.UpdatedAt
            };

            var message = account.IsActive == true ? "Mở khóa tài khoản thành công" : "Khóa tài khoản thành công";

            return Ok(new { Success = true, Message = message, Account = response });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi thay đổi trạng thái tài khoản" });
        }
    }

    // Trong Mo_Api/ApiController/AccountController.cs
    [HttpPost("admin/{id}/grant-seller")]
     [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GrantSellerRole(long id)
    {
        try
        {
            var account = await _accountServices.GrantSellerRoleAsync(id);

            var response = new
            {
                Id = account.Id,
                Username = account.Username,
                CurrentRoles = account.Roles.Select(r => r.RoleName).ToList(),
                UpdatedAt = account.UpdatedAt
            };

            return Ok(new { Success = true, Message = "Cấp phép Seller thành công", Account = response });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi cấp phép Seller" });
        }
    }
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
            }

            var profile = await _accountServices.GetProfileByIdAsync(userId.Value);
            if (profile == null)
            {
                return NotFound(new { Success = false, Message = "User not found" });
            }

            return Ok(new { Success = true, Data = profile });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thông tin profile" });
        }
    }

    [HttpGet("debug-claims")]
    [Authorize]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var userId = User.GetUserId();
        var username = User.GetUsername();
        var email = User.GetEmail();
        var roles = User.GetRoles();
        
        return Ok(new { 
            Claims = claims,
            ExtractedUserId = userId,
            ExtractedUsername = username,
            ExtractedEmail = email,
            ExtractedRoles = roles
        });
    }

    [HttpPut("update-profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var account = await _accountServices.UpdateProfileAsync(userId.Value, request);

            return Ok(new { 
                Success = true, 
                Message = "Cập nhật thông tin thành công",
                Data = new {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    Phone = account.Phone,
                    IdentificationF = account.IdentificationF,
                    IdentificationB = account.IdentificationB,
                    UpdatedAt = account.UpdatedAt
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi cập nhật thông tin" });
        }
    }

    [HttpPost("upload-kyc")]
    [Authorize]
    public async Task<IActionResult> UploadKYC(IFormFile identificationF, IFormFile identificationB)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            if (identificationF == null || identificationB == null)
            {
                return BadRequest(new { Success = false, Message = "Vui lòng upload đầy đủ 2 mặt căn cước" });
            }

            // Validate file types
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            var identificationFExtension = Path.GetExtension(identificationF.FileName).ToLowerInvariant();
            var identificationBExtension = Path.GetExtension(identificationB.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(identificationFExtension) || !allowedExtensions.Contains(identificationBExtension))
            {
                return BadRequest(new { Success = false, Message = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)" });
            }

            // Generate unique filenames
            var identificationFName = $"kyc_f_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{identificationFExtension}";
            var identificationBName = $"kyc_b_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{identificationBExtension}";

            // Save files
            var uploadsPath = Path.Combine("wwwroot", "images", "kyc");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var identificationFPath = Path.Combine(uploadsPath, identificationFName);
            var identificationBPath = Path.Combine(uploadsPath, identificationBName);

            using (var stream = new FileStream(identificationFPath, FileMode.Create))
            {
                await identificationF.CopyToAsync(stream);
            }

            using (var stream = new FileStream(identificationBPath, FileMode.Create))
            {
                await identificationB.CopyToAsync(stream);
            }

            // Update account with KYC URLs
            var identificationFUrl = $"/images/kyc/{identificationFName}";
            var identificationBUrl = $"/images/kyc/{identificationBName}";

            await _accountServices.UpdateKYCAsync(userId.Value, identificationFUrl, identificationBUrl);

            return Ok(new { 
                Success = true, 
                Message = "Upload ảnh KYC thành công",
                Data = new {
                    IdentificationF = identificationFUrl,
                    IdentificationB = identificationBUrl
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi upload ảnh KYC" });
        }
    }


}