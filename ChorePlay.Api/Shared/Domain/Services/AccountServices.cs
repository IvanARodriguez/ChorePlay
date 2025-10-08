using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Jwt;

namespace ChorePlay.Api.Shared.Domain.Services;

public sealed class AccountService(IUserRepository users, IJwtService jwtService)
{
  private readonly IUserRepository _users = users;
  private readonly IJwtService _jwtService = jwtService;

  public async Task<(User user, string accessToken, string refreshToken)> LoginWithGoogleAsync(
    string email, string firstName, string lastName, string picture, CancellationToken ct)
  {
    var user = await _users.UpsertAsync(new User(
            Ulid.NewUlid(),
            email,
            firstName,
            picture,
            lastName
      ), ct);

    var accessToken = _jwtService.GenerateJwtToken(user);
    var refreshToken = _jwtService.GenerateRefreshToken();
    await _users.SaveRefreshTokenAsync(user.Id, refreshToken, ct);

    return (user, accessToken.jwtToken, refreshToken);
  }
}