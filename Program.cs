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
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "MyApi",
                ValidAudience = "MyFrontend",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Very_Long_And_Secret_Key_123!")),
                RoleClaimType = "role",
                NameClaimType = "name"
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

        app.UseAuthentication(); // Who are you?
        app.UseMiddleware<RedirectUnauthorizedMiddleware>(); // Redirect unauthenticated users to Welcome
        app.UseAuthorization();  // What can you do?


        // Seed default admin user
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            if (!db.Users.Any(u => u.Role == ClientForge.Features.User.Models.Role.admin))
            {
                db.Users.Add(new ClientForge.Features.User.Models.User
                {
                    Login = "admin",
                    Name = "Admin",
                    Surname = "Admin",
                    Email = "admin@clientforge.local",
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = ClientForge.Features.User.Models.Role.admin
                });
                db.SaveChanges();
            }
        }

        app.MapStaticAssets();
        app.MapRazorPages()
            .WithStaticAssets();

        app.Run();
    }
}