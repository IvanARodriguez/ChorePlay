namespace ChorePlay.Api.Shared.Domain;

public record User(Ulid Id, string Email, string? FirstName, string? AvatarUrl, string? LastName)
{
  public override string ToString()
  {
    return $"User(Id={Id}, Email={Email}, Name={FirstName} {LastName}, AvatarUrl={AvatarUrl})";
  }
}

