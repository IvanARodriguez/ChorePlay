using ChorePlay.Api.Shared.Domain;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ChorePlay.Api.Shared.Auth;

public static class IdentityExtensions
{
  public static User ToUserDomain(this AppUser appUser)
  {
    return new User(
      Id: appUser.Id,
      Email: appUser.Email ?? string.Empty,
      Name: appUser.UserName ?? string.Empty,
      AvatarUrl: appUser.AvatarUrl
    );
  }
}