using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authApiClient;
    private readonly UserService _userService;
    public AccountController(AuthService authApiClient, UserService userService )
    {
        _authApiClient = authApiClient;
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? success = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        var vm = new LoginVm { ReturnUrl = returnUrl };
        if (!string.IsNullOrEmpty(success))
        {
            vm.Success = success;
        }
        return View(vm);
    }

   
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var res = await _authApiClient.LoginAsync(new AuthService.LoginRequest(vm.Identifier, vm.Password, vm.RememberMe), ct);
        if (res == null)
        {
            vm.Error = "Đăng nhập thất bại";
            return View(vm);
        }

        Response.Cookies.Append("accessToken", res.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Expires = res.ExpiresAt
        });
        Response.Cookies.Append("roles", string.Join(',', res.Roles ?? new()), new CookieOptions
        {
            Expires = res.ExpiresAt
        });

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl)) return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterVm());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        // Kiểm tra mật khẩu xác nhận
        if (vm.Password != vm.ConfirmPassword)
        {
            vm.Error = "Mật khẩu xác nhận không khớp";
            return View(vm);
        }

        try
        {
            var res = await _authApiClient.RegisterAsync(new AuthService.RegisterRequest(vm.Username, vm.Email, vm.Phone, vm.Password), ct);
            if (res == null)
            {
                vm.Error = "Đăng ký thất bại. Tên đăng nhập, email hoặc số điện thoại có thể đã tồn tại.";
                return View(vm);
            }

            vm.Success = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return View(vm);
        }
        catch (Exception ex)
        {
            vm.Error = "Có lỗi xảy ra khi đăng ký: " + ex.Message;
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordVm());
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var success = await _authApiClient.ForgotPasswordAsync(
                new AuthService.ForgotPasswordRequest(vm.Email), ct);

            if (success)
            {
                vm.Success = "Chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra hộp thư.";
                vm.Email = string.Empty; // Clear email for security
            }
            else
            {
                vm.Error = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
            }
        }
        catch (Exception ex)
        {
            vm.Error = "Có lỗi xảy ra: " + ex.Message;
        }

        return View(vm);
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("ForgotPassword");
        }

        return View(new ResetPasswordVm { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var success = await _authApiClient.ResetPasswordAsync(
                new AuthService.ResetPasswordRequest(vm.Token, vm.NewPassword), ct);

            if (success)
            {
                vm.Success = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login", new { success = "Đặt lại mật khẩu thành công!" });
            }
            else
            {
                vm.Error = "Token không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
            }
        }
        catch (Exception ex)
        {
            vm.Error = "Có lỗi xảy ra: " + ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("roles");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ViewProfile()
    {
        try
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            _authApiClient.SetToken(token);
            var profile = await _userService.GetCurrentUserProfileAsync();
            
            if (profile == null)
            {
                ViewBag.Error = "Không thể tải thông tin profile";
                return View(new ProfileVm());
            }

            var vm = new ProfileVm
            {
                Id = profile.Id,
                Username = profile.Username,
                Email = profile.Email,
                Phone = profile.Phone ?? "",
                Balance = profile.Balance ?? 0,
                IsActive = profile.IsActive ?? false,
                CreatedAt = profile.CreatedAt ?? DateTime.Now,
                UpdatedAt = profile.UpdatedAt,
                Roles = profile.Roles,
                IsEKYCVerified = profile.IsEKYCVerified,
                TotalOrders = profile.TotalOrders,
                TotalShops = profile.TotalShops,
                TotalProductsSold = profile.TotalProductsSold,
                IdentificationF = profile.IdentificationF,  // Thêm dòng này
                IdentificationB = profile.IdentificationB   // Thêm dòng này
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
            return View(new ProfileVm());
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(ProfileVm vm)
    {
        try
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View("ViewProfile", vm);
            }

            _authApiClient.SetToken(token);
            
            var updateRequest = new UserService.UpdateProfileRequest(
                vm.Username,
                vm.Email,
                vm.Phone,
                vm.IdentificationF,
                vm.IdentificationB
            );

            Console.WriteLine($"Update Request: Username={vm.Username}, Email={vm.Email}, Phone={vm.Phone}");
            
            var success = await _userService.UpdateProfileAsync(updateRequest);
            
            Console.WriteLine($"Update Result: {success}");
            
            if (success)
            {
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("ViewProfile");
            }
            else
            {
                TempData["Error"] = "Cập nhật thông tin thất bại. Vui lòng thử lại.";
                // Load lại data từ API để giữ nguyên thông tin hiện tại
                vm = await LoadProfileVmAsync();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            // Load lại data từ API để giữ nguyên thông tin hiện tại
            vm = await LoadProfileVmAsync();
        }

        return View("ViewProfile", vm);
    }

    // Helper method để load ProfileVm từ API
    private async Task<ProfileVm> LoadProfileVmAsync()
    {
        try
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return new ProfileVm();
            }

            _authApiClient.SetToken(token);
            var profile = await _userService.GetCurrentUserProfileAsync();
            
            if (profile == null)
            {
                return new ProfileVm();
            }

            return new ProfileVm
            {
                Id = profile.Id,
                Username = profile.Username,
                Email = profile.Email,
                Phone = profile.Phone ?? "",
                Balance = profile.Balance ?? 0,
                IsActive = profile.IsActive ?? false,
                CreatedAt = profile.CreatedAt ?? DateTime.Now,
                UpdatedAt = profile.UpdatedAt,
                Roles = profile.Roles,
                IsEKYCVerified = profile.IsEKYCVerified,
                TotalOrders = profile.TotalOrders,
                TotalShops = profile.TotalShops,
                TotalProductsSold = profile.TotalProductsSold,
                IdentificationF = profile.IdentificationF,
                IdentificationB = profile.IdentificationB
            };
        }
        catch
        {
            return new ProfileVm();
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadKYC(IFormFile identificationF, IFormFile identificationB)
    {
        try
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này" });
            }

            if (identificationF == null || identificationB == null)
            {
                return Json(new { success = false, message = "Vui lòng chọn đầy đủ 2 ảnh" });
            }

            _authApiClient.SetToken(token);
            var success = await _userService.UploadKYCAsync(identificationF, identificationB);
            
            if (success)
            {
                return Json(new { success = true, message = "Upload ảnh KYC thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Upload ảnh KYC thất bại. Vui lòng thử lại." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }

}


