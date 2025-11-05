using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.OpenApi.Models;
using Mo_DataAccess.Services;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers().AddOData(options =>
        {
            options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100);
        });
        // Enable HttpClient factory for services that need HttpClient (e.g., VnpayTransactionServices)
        builder.Services.AddHttpClient();
        // Register CORS services
        builder.Services.AddCors();
        builder.Services.AddDbContext<SwpGroup6Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        // Hangfire configuration using the same SQL Server connection
        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                  {
                      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                      QueuePollInterval = TimeSpan.FromSeconds(5),
                      UseRecommendedIsolationLevel = true,
                      DisableGlobalLocks = true
                  });
        });
        builder.Services.AddHangfireServer();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt Key not found")))
                };
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectGroup6 API", Version = "v1" });
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Nhập 'Bearer {token}'",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, new string[] { } }
            });
        });
        builder.Services.AddScoped<IAccountServices, AccountServices>();
        builder.Services.AddScoped<ICategoryServices, CategoryServices>();
        builder.Services.AddScoped<ISubCategoryServices, SubCategoryServices>();
        builder.Services.AddScoped<IFeedbackServices, FeedbackServices>();
        builder.Services.AddScoped<IImageMessageServices, ImageMessageServices>();
        builder.Services.AddScoped<IMessageServices, MessageServices>();
        builder.Services.AddScoped<IOrderProductServices, OrderProductServices>();
        builder.Services.AddScoped<IPaymentTransactionServices, PaymentTransactionServices>();
        builder.Services.AddScoped<IProductServices, ProductServices>();
        builder.Services.AddScoped<IProductStoreServices, ProductStoreServices>();
        builder.Services.AddScoped<IProductVariantServices, ProductVariantServices>();
        builder.Services.AddScoped<IReplyServices, ReplyServices>();
        builder.Services.AddScoped<IRoleServices, RoleServices>();
        builder.Services.AddScoped<IShopServices, ShopServices>();
        builder.Services.AddScoped<ISupportTicketServices, SupportTicketServices>();
        builder.Services.AddScoped<ISystemsConfigServices, SystemsConfigServices>();
        builder.Services.AddScoped<ITextMessageServices, TextMessageServices>();
        builder.Services.AddScoped<ITokenServices, TokenServices>();
        builder.Services.AddScoped<IVnpayTransactionServices, VnpayTransactionServices>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddHttpClient<VnpayTransactionServices>();
        builder.Services.AddAutoMapper(typeof(Program));
       // builder.Services.AddHostedService<SellerPayoutBackgroundService>();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        // Hangfire dashboard (optional: protect with auth in production)
        app.UseHangfireDashboard("/hangfire");

        app.UseHttpsRedirection();
        app.UseCors(options =>
        {
            options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
        app.UseAuthentication();
        app.UseAuthorization();
         app.UseCors(options =>
         {
              options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });

        app.MapControllers();

        app.Run();
    }
}