using Microsoft.AspNetCore.Antiforgery;
using Mo_Client.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("MoApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7234/"); // hoặc port của Mo_Api
});

builder.Services.AddControllersWithViews();


// ✅ Cấu hình antiforgery cho HTTPS cross-site
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


// API options
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register API Services
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<UserService>();
builder.Services.AddHttpClient<AdminService>();
builder.Services.AddHttpClient<AuthApiClient>();
builder.Services.AddHttpClient<CategoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});
app.UseRouting();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
    if (HttpMethods.IsGet(context.Request.Method))
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
    }
    await next.Invoke();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ApiOptions { public string BaseUrl { get; set; } = string.Empty; }
