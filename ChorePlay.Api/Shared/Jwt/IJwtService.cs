using System.Security.Claims;

namespace ChorePlay.Api.Shared.Jwt;

public interface IJwtService
{
  string CreateJwtAccessToken(Ulid User, string Email, IEnumerable<string>? roles = null);
  string GenerateRefreshToken();
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string Token);
}