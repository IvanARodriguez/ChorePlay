using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Domain;
using ChorePlay.Api.Shared.Jwt;
using Mediator;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginHandler(
    IJwtService jwtService,
    IUserRepository users
) : ICommandHandler<GoogleLoginCommand, GoogleLoginResponse>
{
  private readonly IJwtService _jwtService = jwtService;
  private readonly IUserRepository _users = users;

  public async ValueTask<GoogleLoginResponse> Handle(GoogleLoginCommand command, CancellationToken ct)
  {

    var payload = command.Payload;
    var email = payload.Email ?? throw new UnauthorizedAccessException("Email not provided by Google");
    var firstName = payload.GivenName ?? string.Empty;
    var picture = payload.Picture ?? string.Empty;
    var LastName = payload.FamilyName ?? string.Empty;


    // find or create user
    var user = await _users.FindByEmailAsync(email, ct)
               ?? await _users.CreateWithGoogleAsync(new User(Ulid.NewUlid(), email, firstName, picture, LastName), ct);

    // issue tokens
    var accessToken = _jwtService.CreateJwtAccessToken(user.Id, user.Email);
    var refreshToken = _jwtService.GenerateRefreshToken();
    await _users.SaveRefreshTokenAsync(user.Id, refreshToken, ct);

    return new GoogleLoginResponse(user.Id, user.Email, accessToken, refreshToken);
  }
}
