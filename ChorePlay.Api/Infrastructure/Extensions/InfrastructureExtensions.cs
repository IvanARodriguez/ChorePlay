using System.Text;
using ChorePlay.Api.Infrastructure.Data;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Configuration;
using ChorePlay.Api.Shared.Email;
using ChorePlay.Api.Shared.Jwt;
using ChorePlay.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChorePlay.Api.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services (Database, Identity, Authentication).
/// </summary>
public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddAuthenticationServices(configuration);
        services.AddEmailServices(configuration);
        services.AddDatabase(configuration);
        services.AddIdentityServices();
        services.AddCookieServices();

        return services;
    }

    private static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<SesSettings>(configuration.GetSection("Ses"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SesSettings>>().Value);
        services.AddSingleton<IEmailService, SesEmailService>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                // Password requirements
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        services.AddScoped<IJwtService, JwtService>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId =
                    configuration["Authentication:Google:ClientId"]
                    ?? throw new ArgumentNullException("Google Client ID is required");
                googleOptions.ClientSecret =
                    configuration["Authentication:Google:ClientSecret"]
                    ?? throw new ArgumentNullException("Google Client Secret is required");

                googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
                googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
                googleOptions.ClaimActions.MapJsonKey("picture", "picture");
            })
            .AddJwtBearer(opt =>
            {
                var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
                opt.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)
                    ),
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddCookieServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CookieManager>();

        return services;
    }
}
