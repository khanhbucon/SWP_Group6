using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Mo_DataAccess;
using Mo_DataAccess.Services;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using System.Text;

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
        builder.Services.AddDbContext<SwpGroup6Context>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
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
        builder.Services.AddScoped<IFeedbackServices, FeedbackServices>();
        builder.Services.AddScoped<IImageMessageServices, ImageMessageServices>();
        builder.Services.AddScoped<IMessageServices, MessageServices>();
        builder.Services.AddScoped<IOrderProductProductStoreServices, OrderProductProductStoreServices>();
        builder.Services.AddScoped<IOrderProductServices, OrderProductServices>();
        builder.Services.AddScoped<IPaymentTransactionServices, PaymentTransactionServices>();
        builder.Services.AddScoped<IProductServices, ProductServices>();
        builder.Services.AddScoped<IProductStoreServices, ProductStoreServices>();
        builder.Services.AddScoped<IProductVariantServices, ProductVariantServices>();
        builder.Services.AddScoped<IReplyServices, ReplyServices>();
        builder.Services.AddScoped<IRoleServices, RoleServices>();
        builder.Services.AddScoped<IShopServices, ShopServices>();
        builder.Services.AddScoped<ISubCategoryServices, SubCategoryServices>();
        builder.Services.AddScoped<ISupportTicketServices, SupportTicketServices>();
        builder.Services.AddScoped<ISystemsConfigServices, SystemsConfigServices>();
        builder.Services.AddScoped<ITextMessageServices, TextMessageServices>();
        builder.Services.AddScoped<ITokenServices, TokenServices>();
        builder.Services.AddScoped<IVnpayTransactionServices, VnpayTransactionServices>();
        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<ProductRepository>();
        builder.Services.AddScoped<CategoryRepository>();
       





        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
         app.UseCors(options =>
         {
              options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
         })  ;

        app.MapControllers();

        app.Run();
    }
}