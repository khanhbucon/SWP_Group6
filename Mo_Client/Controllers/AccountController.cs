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

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("roles");
        return RedirectToAction("Login");
    }
}


