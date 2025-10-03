using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Domain;

namespace ChorePlay.Api.Shared.Mappings;

public static class IdentityExtensions
{
  public static User ToUserDomain(this AppUser appUser)
  {
    ArgumentNullException.ThrowIfNull(appUser);

    return new User(
        Id: appUser.Id,
        Email: appUser.Email ?? string.Empty,
        FirstName: appUser.FirstName ?? string.Empty,
        AvatarUrl: appUser.AvatarUrl,
        LastName: appUser.LastName
    );
  }
}
