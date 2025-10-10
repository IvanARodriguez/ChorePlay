using ChorePlay.Api.Shared.Domain.Services;
using Mediator;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginHandler(AccountService accountService)
  : ICommandHandler<GoogleLoginCommand, GoogleLoginResponse>
{
  public async ValueTask<GoogleLoginResponse> Handle(GoogleLoginCommand command, CancellationToken ct)
  {
    var payload = command.Payload;
    var email = payload.Email ?? throw new UnauthorizedAccessException("Email not provided by Google");

    var result = await accountService.LoginOrRegisterWithGoogleAsync(
      email,
      payload.GivenName ?? string.Empty,
      payload.FamilyName ?? string.Empty,
      payload.Picture ?? string.Empty,
      ct
    );

    return new GoogleLoginResponse(result.UserId, result.Email, result.AccessToken, result.RefreshToken);
  }
}
