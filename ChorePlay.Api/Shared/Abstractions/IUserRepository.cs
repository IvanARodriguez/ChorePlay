using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Domain;
using ChorePlay.Api.Shared.Jwt;

namespace ChorePlay.Api.Shared.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<User> UpsertAsync(User user, CancellationToken ct);
    Task SaveRefreshTokenAsync(Ulid userId, string refreshToken, DateTime expirationInUtc, CancellationToken ct);
    Task<User> CreateAsync(User user, CancellationToken ct);
    Task<bool> PasswordIsValid(User user, string password, CancellationToken ct);
}
