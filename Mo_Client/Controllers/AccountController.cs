using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class AccountController : Controller
{
    private readonly AuthApiClient _authApiClient;
    public AccountController(AuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

   
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var res = await _authApiClient.LoginAsync(new AuthApiClient.LoginRequest(vm.Identifier, vm.Password, vm.RememberMe), ct);
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
            var res = await _authApiClient.RegisterAsync(new AuthApiClient.RegisterRequest(vm.Username, vm.Email, vm.Phone, vm.Password), ct);
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

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("roles");
        return RedirectToAction("Login");
    }
}


