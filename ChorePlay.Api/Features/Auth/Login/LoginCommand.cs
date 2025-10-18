using Mediator;

namespace ChorePlay.Api.Features.Auth.Login;

public sealed record LoginCommand(LoginRequest Request) : ICommand<LoginResponse>;
