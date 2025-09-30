using System.Security.Claims;

namespace ChorePlay.Api.Shared.Jwt;

public class JwtService : IJwtService
{
  public string CreateJwtAccessToken(string User, IEnumerable<string>? roles = null)
  {
    throw new NotImplementedException();
  }

  public string GenerateRefreshToken()
  {
    throw new NotImplementedException();
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string Token)
  {
    throw new NotImplementedException();
  }
}