using ChorePlay.Api.Shared.Domain;

namespace ChorePlay.Api.Features.Auth.Login;

public record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);
