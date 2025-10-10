using System.Text;
using ChorePlay.Api.Infrastructure.Data;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Configuration;
using ChorePlay.Api.Shared.Jwt;
using ChorePlay.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
      IConfiguration configuration)
  {
    services.AddDatabase(configuration);
    services.AddIdentityServices();
    services.AddAuthenticationServices(configuration);
    services.AddCookieServices();

    return services;
  }

  private static IServiceCollection AddDatabase(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    return services;
  }

  private static IServiceCollection AddIdentityServices(this IServiceCollection services)
  {
    services.AddIdentity<AppUser, AppRole>(options =>
    {
      // Password requirements
      options.Password.RequireDigit = true;
      options.Password.RequireLowercase = true;
      options.Password.RequireNonAlphanumeric = true;
      options.Password.RequireUppercase = true;
      options.Password.RequiredLength = 8;

      options.Lockout.MaxFailedAccessAttempts = 5;
      options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

      // User requirements
      options.User.RequireUniqueEmail = true;

      // Sign-in requirements
      options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    return services;
  }

  private static IServiceCollection AddAuthenticationServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // JWT Configuration
    services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

    services.AddSingleton(sp =>
        sp.GetRequiredService<IOptions<JwtSettings>>().Value);

    services.AddScoped<IJwtService, JwtService>();

    // Google OAuth Configuration
    services.Configure<GoogleAuthSettings>(options =>
    {
      options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? string.Empty;
      options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? string.Empty;
    });

    services.AddAuthentication(opt =>
    {
      opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
        .AddGoogle(googleOptions =>
        {
          var clientId = configuration["Authentication:Google:ClientId"];
          if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId), "Google Client ID is required");

          var clientSecret = configuration["Authentication:Google:ClientSecret"];
          if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret), "Google Client Secret is required");

          googleOptions.ClientId = clientId;
          googleOptions.ClientSecret = clientSecret;
          googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
          googleOptions.ClaimActions.MapJsonKey("urn:google:profile", "link");
          googleOptions.ClaimActions.MapJsonKey("picture", "picture");
        })
        .AddCookie(options =>
        {
          var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
          options.Cookie.Name = "auth_token";
          options.Cookie.HttpOnly = true;
          options.Cookie.SameSite = SameSiteMode.Strict;
          options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
          options.SlidingExpiration = true;
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
          };
          opt.Events = new()
          {
            OnMessageReceived = context =>
            {
              context.Token = context.Request.Cookies["ACCESS_TOKEN"];
              return Task.CompletedTask;
            }
          };
        });

    services.ConfigureApplicationCookie(options =>
    {
      options.ExpireTimeSpan = TimeSpan.FromDays(7);
      options.SlidingExpiration = true;
      options.Cookie.SameSite = SameSiteMode.None; // Important for cross-site redirect flow
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use HTTPS when possible
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