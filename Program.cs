using Microsoft.EntityFrameworkCore;
using ClientForge.Data;
using ClientForge.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace ClientForge;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages
        (
            options =>
            {
                options.RootDirectory = "/";
                options.Conventions.AddFolderRouteModelConvention
                (
                    "/",
                    model =>
                    {
                        foreach (var selector in model.Selectors)
                        {
                            var template = selector.AttributeRouteModel?.Template;
                            if (!string.IsNullOrEmpty(template))
                            {
                                template = template.Replace("Features/", "", StringComparison.OrdinalIgnoreCase);
                                template = template.Replace("/Pages", "", StringComparison.OrdinalIgnoreCase);
                                template = template.Replace("Pages", "", StringComparison.OrdinalIgnoreCase);

                                if (template.StartsWith("Pages/", StringComparison.OrdinalIgnoreCase))
                                {
                                    template = template.Replace("Pages/", "", StringComparison.OrdinalIgnoreCase);
                                }

                                selector.AttributeRouteModel!.Template = template;
                            }
                        }
                    }
                );
            }
        );

        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "MyApi",
                ValidAudience = "MyFrontend",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Very_Long_And_Secret_Key_123!"))
            };
            // Read JWT from the auth_token cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("auth_token"))
                    {
                        context.Token = context.Request.Cookies["auth_token"];
                    }
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        app.UseRouting();

        app.UseAuthentication(); // Кто ты?
        app.UseMiddleware<RedirectUnauthorizedMiddleware>(); // Редирект неавторизованных на Welcome
        app.UseAuthorization();  // Что тебе можно?


        app.MapStaticAssets();
        app.MapRazorPages()
            .WithStaticAssets();

        app.Run();
    }
}