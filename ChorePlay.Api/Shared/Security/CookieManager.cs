namespace ChorePlay.Api.Shared.Security;

public class CookieManager(IHttpContextAccessor accessor, IWebHostEnvironment env)
{
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;
    private readonly IWebHostEnvironment _env = env;

    public void SetRefreshToken(string refreshToken, DateTime expiresAtUtc)
    {
        var options = new CookieOptions { HttpOnly = true, Expires = expiresAtUtc };

        if (_env.IsDevelopment())
        {
            options.Secure = false;
            options.SameSite = SameSiteMode.Lax;
        }
        else
        {
            options.Secure = true;
            options.SameSite = SameSiteMode.None;
        }

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            options
        );
    }

    public string? GetRefreshToken()
    {
        _httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(
            "refreshToken",
            out var token
        );
        return token;
    }

    public void ClearRefreshToken()
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Delete(
            "refreshToken",
            new CookieOptions { Path = "/api/auth/refresh" }
        );
    }
}
