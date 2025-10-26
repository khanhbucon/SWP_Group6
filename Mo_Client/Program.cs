using Mo_Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// API options
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register API Services
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<UserService>();
builder.Services.AddHttpClient<AdminService>();
builder.Services.AddHttpClient<CategoryService>();
builder.Services.AddHttpClient<TransactionService>();
builder.Services.AddHttpClient<OrderService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ApiOptions { public string BaseUrl { get; set; } = string.Empty; }
