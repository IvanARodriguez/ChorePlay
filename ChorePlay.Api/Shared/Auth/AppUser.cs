using Microsoft.AspNetCore.Identity;

namespace ChorePlay.Api.Shared.Auth;

public class AppUser : IdentityUser<Ulid>
{
  // Profile info
  public string? AvatarUrl { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }

  // External login tracking
  public string? Provider { get; set; }      // e.g. "Google"
  public string? ProviderId { get; set; }    // external subject identifier

  // Refresh token management
  public string? RefreshToken { get; set; }
  public DateTime? RefreshTokenExpirationDate { get; set; }
}
