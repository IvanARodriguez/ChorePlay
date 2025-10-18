namespace ChorePlay.Api.Features.Auth.DTOs;

public record AuthResultsDto(
    Ulid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool EmailConfirmed,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);
