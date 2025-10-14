using Mo_Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// API options
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));

// Register API Services
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<UserService>();
builder.Services.AddHttpClient<AdminService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ApiOptions { public string BaseUrl { get; set; } = string.Empty; }
