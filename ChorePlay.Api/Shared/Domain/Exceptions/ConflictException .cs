namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource).
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with entity name and key.
    /// </summary>
    /// <param name="name">The name of the entity (e.g., "User", "Email").</param>
    /// <param name="key">The key/identifier that conflicts.</param>
    public ConflictException(string name, object key)
        : base($"{name} with key '{key}' already exists.")
    {
        EntityName = name;
        Key = key;
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public string? EntityName { get; }

    public object? Key { get; }
}
