using Google.Apis.Auth;
using Mediator;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public sealed record GoogleLoginCommand(GoogleJsonWebSignature.Payload Payload)
    : ICommand<GoogleLoginResponse>;
