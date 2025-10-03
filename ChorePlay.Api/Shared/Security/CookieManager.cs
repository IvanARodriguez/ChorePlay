namespace ChorePlay.Api.Shared.Security;

public class CookieManager(IHttpContextAccessor accessor, IWebHostEnvironment env)
{
  private readonly IHttpContextAccessor _httpContextAccessor = accessor;
  private readonly IWebHostEnvironment _env = env;

  public void SetRefreshToken(string refreshToken)
  {
    var options = new CookieOptions
    {
      HttpOnly = true,
      MaxAge = TimeSpan.FromDays(15),
    };

    if (_env.IsDevelopment())
    {
      options.Secure = false;
      options.SameSite = SameSiteMode.Lax; // avoids warning in dev
    }
    else
    {
      options.Secure = true;
      options.SameSite = SameSiteMode.None;
    }

    _httpContextAccessor.HttpContext!.Response.Cookies.Append(
      "refreshToken",
      refreshToken,
      options);
  }

  public string? GetRefreshToken()
  {
    _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var token);
    return token;
  }

  public void ClearRefreshToken()
  {
    _httpContextAccessor.HttpContext!.Response.Cookies.Delete("refreshToken", new CookieOptions
    {
      Path = "/api/auth/refresh"
    });
  }
}