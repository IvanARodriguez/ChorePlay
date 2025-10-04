namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// Initializes a new instance with an error message.
    /// </summary>
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with an error message and inner exception.
    /// </summary>
    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance with rule name and details.
    /// </summary>
    /// <param name="ruleName">The name of the business rule that was violated.</param>
    /// <param name="details">Additional details about the violation.</param>
    public BusinessRuleViolationException(string ruleName, string details)
        : base($"Business rule '{ruleName}' was violated: {details}")
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// The name of the business rule that was violated.
    /// </summary>
    public string? RuleName { get; }
}

