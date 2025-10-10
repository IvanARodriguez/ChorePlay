namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public record GoogleLoginResponse(
  Ulid Id,
  string Email,
  string? AccessToken,
  string? RefreshToken
);
