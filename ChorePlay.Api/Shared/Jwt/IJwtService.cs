using System.Security.Claims;

namespace ChorePlay.Api.Shared.Jwt;

public interface IJwtService
{
  string CreateJwtAccessToken(string User, IEnumerable<string>? roles = null);
  string GenerateRefreshToken();
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string Token);
}