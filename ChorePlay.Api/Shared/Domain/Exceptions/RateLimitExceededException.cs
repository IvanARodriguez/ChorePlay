namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when rate limiting is exceeded.
/// </summary>
public class RateLimitExceededException : Exception
{
    /// <summary>
    /// Initializes a new instance with retry-after seconds.
    /// </summary>
    /// <param name="retryAfterSeconds">Number of seconds before retry is allowed.</param>
    public RateLimitExceededException(int retryAfterSeconds)
        : base($"Rate limit exceeded. Please try again in {retryAfterSeconds} seconds.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public RateLimitExceededException(string message, int retryAfterSeconds)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; }
}