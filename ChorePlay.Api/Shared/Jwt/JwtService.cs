using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChorePlay.Api.Shared.Jwt;

public class RefreshToken
{
  public string Token { get; init; } = string.Empty;
  public DateTime Expires { get; init; }
  public DateTime Created { get; init; }
}

public class JwtService(JwtSettings settings) : IJwtService
{
  private readonly JwtSettings _settings = settings;

  public string CreateJwtAccessToken(Ulid userId, string email, IEnumerable<string>? roles = null)
  {
    var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email)
        };

    if (roles != null)
    {
      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _settings.Issuer,
        audience: _settings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiresInMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);

    return Convert.ToBase64String(randomNumber);
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = true,
      ValidateIssuer = true,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
      ValidateLifetime = false, // ⚡ allow expired token
      ValidIssuer = _settings.Issuer,
      ValidAudience = _settings.Audience
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

    if (securityToken is not JwtSecurityToken jwtSecurityToken ||
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
    {
      throw new SecurityTokenException("Invalid token");
    }

    return principal;
  }

}
