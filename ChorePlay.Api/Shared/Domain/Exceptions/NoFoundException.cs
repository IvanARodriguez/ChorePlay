namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with entity name and key.
    /// </summary>
    /// <param name="name">The name of the entity (e.g., "User", "Chore").</param>
    /// <param name="key">The key/identifier that was not found.</param>
    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
        EntityName = name;
        Key = key;
    }

    /// <summary>
    /// The name of the entity that was not found.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// The key/identifier that was not found.
    /// </summary>
    public object? Key { get; }
}
