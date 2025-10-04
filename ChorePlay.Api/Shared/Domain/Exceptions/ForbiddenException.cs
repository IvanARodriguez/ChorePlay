namespace ChorePlay.Api.Shared.Domain.Exceptions;
/// <summary>
/// Exception thrown when authorization fails.
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance with default message.
    /// </summary>
    public ForbiddenException()
        : base("You do not have permission to access this resource.")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with resource and required permission.
    /// </summary>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="requiredPermission">The permission required.</param>
    public ForbiddenException(string resource, string requiredPermission)
        : base($"You do not have '{requiredPermission}' permission to access '{resource}'.")
    {
        Resource = resource;
        RequiredPermission = requiredPermission;
    }

    public string? Resource { get; }
    public string? RequiredPermission { get; }
}
