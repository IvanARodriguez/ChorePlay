using System.Security.Claims;
using ChorePlay.Api.Shared.Domain;

namespace ChorePlay.Api.Shared.Jwt;

public interface IJwtService
{
    (string accessToken, DateTime expiresAtUtc) GenerateJwtToken(User user);
    (string refreshToken, DateTime expiresAtUtc) GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string Token);
    void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration);
    string HashToken(string token);
}
