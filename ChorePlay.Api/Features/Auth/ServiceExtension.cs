using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Infrastructure.Repository;

namespace ChorePlay.Api.Features.Auth;

/// <summary>
/// Extension methods for configuring Auth feature services.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers all Auth feature services and dependencies.
    /// </summary>
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Add other auth-related services here as you build them:
        // services.AddScoped<ITokenRefreshService, TokenRefreshService>();
        // services.AddScoped<IPasswordResetService, PasswordResetService>();
        // services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        return services;
    }
}