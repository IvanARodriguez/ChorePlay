namespace ChorePlay.Api.Features.Auth.Register;

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LasName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
