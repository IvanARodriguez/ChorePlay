using ChorePlay.Api.Shared.Domain;

namespace ChorePlay.Api.Shared.Auth;

public static class IdentityExtensions
{
  public static User ToUserDomain(this AppUser appUser)
  {
    ArgumentNullException.ThrowIfNull(appUser);

    return new User(
        Id: appUser.Id,
        Email: appUser.Email ?? string.Empty,
        FirstName: appUser.FirstName ?? string.Empty,
        LastName: appUser.LastName ?? string.Empty,
        AvatarUrl: appUser.AvatarUrl
    )
    {
      EmailConfirmed = appUser.EmailConfirmed,
      OAuthEmailConfirmed = appUser.OAuthEmailConfirmed
    };
  }

  public static AppUser ToAppUserDomain(this User user)
  {
    ArgumentNullException.ThrowIfNull(user);

    return new AppUser
    {
      Id = user.Id,
      Email = user.Email ?? string.Empty,
      FirstName = user.FirstName ?? string.Empty,
      LastName = user.LastName ?? string.Empty,
      AvatarUrl = user.AvatarUrl,
      EmailConfirmed = user.EmailConfirmed,
      OAuthEmailConfirmed = user.OAuthEmailConfirmed
    };
  }
}

