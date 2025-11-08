using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class PaymentController : Controller
{
    private readonly DepositService _depositService;
    private readonly IConfiguration _configuration;

    public PaymentController(DepositService depositService, IConfiguration configuration)
    {
        _depositService = depositService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Deposit()
    {
        // Kiểm tra token
        var token = Request.Cookies["accessToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Payment/Deposit" });
        }

        // Set token cho service
        _depositService.SetToken(token);

        // Lấy API base URL
        var apiBaseUrl = _configuration["ApiOptions:BaseUrl"] ?? "https://localhost:7234";
        if (!apiBaseUrl.EndsWith("/api"))
        {
            apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/api";
        }
        ViewBag.ApiBaseUrl = apiBaseUrl;
        ViewBag.AccessToken = token; // Pass token to view since cookie is HttpOnly

        return View();
    }
}

