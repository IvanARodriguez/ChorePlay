using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChorePlay.Api.Shared.Domain;
using Microsoft.IdentityModel.Tokens;

namespace ChorePlay.Api.Shared.Jwt;

public class JwtService(JwtSettings settings, IHttpContextAccessor httpContextAccessor) : IJwtService
{
  private readonly JwtSettings _settings = settings;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration)
  {
    _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, token, new CookieOptions
    {
      HttpOnly = true,
      Expires = expiration,
      IsEssential = true,
      Secure = true,
      SameSite = SameSiteMode.Strict
    });
  }

  public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
  {
    var signingKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_settings.Secret)
    );
    var credentials = new SigningCredentials(
      signingKey,
      SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Sid, Ulid.NewUlid().ToString()),
      new Claim(JwtRegisteredClaimNames.Email, user.Email),
      new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
      new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
      new Claim(ClaimTypes.NameIdentifier, user.ToString())
    };

    var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiresInMinutes);

    var token = new JwtSecurityToken(
      issuer: _settings.Issuer,
      audience: _settings.Audience,
      claims: claims,
      expires: expires,
      signingCredentials: credentials
    );

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

    return (jwtToken, expires);
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
