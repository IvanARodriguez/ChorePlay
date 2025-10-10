namespace ChorePlay.Api.Features.Auth.DTOs;

public record AuthResultDto
(
    Ulid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool EmailConfirmed,
    bool OAuthEmailConfirmed,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);