using Microsoft.AspNetCore.Identity;

namespace ChorePlay.Api.Shared.Auth;

public class AppRole : IdentityRole<Ulid>
{
  public AppRole() : base() { }
  public AppRole(string roleName) : base(roleName) { }
}