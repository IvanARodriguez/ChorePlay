using ChorePlay.Api.Infrastructure.Repository;
using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Domain.Services;

namespace ChorePlay.Api.Features.Auth;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<AccountService>();

        // Add other auth-related services here as you build them:
        // services.AddScoped<ITokenRefreshService, TokenRefreshService>();
        // services.AddScoped<IPasswordResetService, PasswordResetService>();
        // services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        return services;
    }
}
