namespace ChorePlay.Api.Shared.Jwt;

public class JwtSettings
{
  public string Secret { get; set; } = null!;
  public string Issuer { get; set; } = null!;
  public string Audience { get; set; } = null!;
  public int AccessTokenExpiresInMinutes { get; set; } = 15;
  public int RefreshTokenExpiresInDays { get; set; } = 15;
}

