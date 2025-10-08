using System.Security.Claims;
using ChorePlay.Api.Shared.Domain;

namespace ChorePlay.Api.Shared.Jwt;

public interface IJwtService
{
  public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user);
  string GenerateRefreshToken();
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string Token);
  void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration);
}