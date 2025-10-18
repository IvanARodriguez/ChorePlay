using System.Text;
using ChorePlay.Api.Infrastructure.Data;
using ChorePlay.Api.Infrastructure.Repository;
using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Configuration;
using ChorePlay.Api.Shared.Jwt;
using ChorePlay.Api.Shared.Security;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChorePlay.Api.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureGoogleAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<GoogleAuthSettings>(options =>
        {
            options.ClientId =
                Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? string.Empty;
            options.ClientSecret =
                Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? string.Empty;
        });

        return services;
    }

    public static IServiceCollection ConfigureJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    public static IServiceCollection ConfigureCookies(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CookieManager>();

        return services;
    }

    public static IServiceCollection ConfigureDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }

    public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection ConfigureAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication()
            .AddGoogle(googleOptions =>
            {
                var clientId = configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                    throw new ArgumentNullException(
                        nameof(clientId),
                        "Google Client ID is required"
                    );

                var clientSecret = configuration["Authentication:Google:ClientSecret"];
                if (string.IsNullOrEmpty(clientSecret))
                    throw new ArgumentNullException(
                        nameof(clientSecret),
                        "Google Client Secret is required"
                    );

                googleOptions.ClientId = clientId;
                googleOptions.ClientSecret = clientSecret;
                googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
                googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
                googleOptions.ClaimActions.MapJsonKey("picture", "picture");
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
                options.TokenValidationParameters = new TokenValidationParameters
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

        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    public static IServiceCollection ConfigureAuthorizationServices(
        this IServiceCollection services
    )
    {
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        // Add more repositories here as needed
        return services;
    }

    public static IServiceCollection ConfigureMediator(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

        return services;
    }
}
