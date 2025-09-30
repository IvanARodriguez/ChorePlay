namespace ChorePlay.Api.Shared.Domain;

public record User(Ulid Id, string Email, string Name, string? AvatarUrl);