namespace ChorePlay.Api.Features.Auth.Login;

public record LoginRequest
{
  public required string Email { get; init; }
  public required string Password { get; init; }
}