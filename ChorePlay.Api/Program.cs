using System.Text;
using ChorePlay.Api.Data;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Jwt;
using ChorePlay.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mediator;
using ChorePlay.Api.Features.Auth.GoogleLogin;
using ChorePlay.Api.Shared.Configuration;
using DotNetEnv;
using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Add env + appsettings.json
builder.Configuration.AddEnvironmentVariables();

// Bind Google auth settings
builder.Services.Configure<GoogleAuthSettings>(options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? string.Empty;
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? string.Empty;
});

// Jwt
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<JwtSettings>>().Value);

builder.Services.AddScoped<IJwtService, JwtService>();

// Cookies
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CookieManager>();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Auth with Google
builder.Services.AddAuthentication()
.AddGoogle(googleOptions =>
{
    var clientId = builder.Configuration["Authentication:Google:ClientId"];
    if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
    var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException(nameof(clientSecret));

    googleOptions.ClientId = clientId;
    googleOptions.ClientSecret = clientSecret;
    googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.None; // important for cross-site redirect flow
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // use HTTPS when possible
});

builder.Services.AddAuthorization();

// Repositories 
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Mediator
builder.Services.AddMediator(options =>
    options.ServiceLifetime = ServiceLifetime.Scoped);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "ChorePlay API is running");
// Endppoints
app.MapGoogleLogin();
app.Run();

