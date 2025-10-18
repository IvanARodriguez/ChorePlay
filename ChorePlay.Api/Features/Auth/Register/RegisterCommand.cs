using Mediator;

namespace ChorePlay.Api.Features.Auth.Register;

public sealed record RegisterCommand(RegisterRequest Request) : ICommand<RegisterResponse>;
