using Mo_Client.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("MoApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7234/"); // hoặc port của Mo_Api
});

builder.Services.AddControllersWithViews();

// API options
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<AuthApiClient>();

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
