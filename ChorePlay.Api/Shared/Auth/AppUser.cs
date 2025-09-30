using Microsoft.AspNetCore.Identity;

namespace ChorePlay.Api.Shared.Auth;

public class AppUser : IdentityUser<Ulid>
{
  public string? AvatarUrl { get; set; }
  public string? ProviderId { get; set; }
}