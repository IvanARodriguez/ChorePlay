using ChorePlay.Api.Shared.Abstractions;
using ChorePlay.Api.Shared.Domain;
using ChorePlay.Api.Shared.Domain.Services;
using ChorePlay.Api.Shared.Jwt;
using Mediator;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginHandler(
   AccountService accountService
) : ICommandHandler<GoogleLoginCommand, GoogleLoginResponse>
{
  private readonly AccountService _accountService = accountService;

  public async ValueTask<GoogleLoginResponse> Handle(GoogleLoginCommand command, CancellationToken ct)
  {

    var payload = command.Payload;
    var email = payload.Email ?? throw new UnauthorizedAccessException("Email not provided by Google");
    var firstName = payload.GivenName ?? string.Empty;
    var picture = payload.Picture ?? string.Empty;
    var LastName = payload.FamilyName ?? string.Empty;


    // find or create user
    var (user, accessToken, refreshToken) = await _accountService.LoginWithGoogleAsync(
           email,
           payload.GivenName ?? string.Empty,
           payload.FamilyName ?? string.Empty,
           payload.Picture ?? string.Empty,
           ct
       );

    return new GoogleLoginResponse(user.Id, user.Email, accessToken, refreshToken);
  }
}
