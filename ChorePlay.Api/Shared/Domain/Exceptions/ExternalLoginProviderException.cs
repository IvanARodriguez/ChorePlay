namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when external login provider authentication fails.
/// </summary>
public class ExternalLoginProviderException : Exception
{
  /// <summary>
  /// Initializes a new instance with provider name and error message.
  /// </summary>
  /// <param name="provider">The name of the external provider (e.g., "Google", "Facebook").</param>
  /// <param name="message">The error message describing what went wrong.</param>
  public ExternalLoginProviderException(string provider, string message)
      : base($"External login failed for provider '{provider}': {message}")
  {
    Provider = provider;
  }

  /// <summary>
  /// Initializes a new instance with provider name and inner exception.
  /// </summary>
  public ExternalLoginProviderException(string provider, string message, Exception innerException)
      : base($"External login failed for provider '{provider}': {message}", innerException)
  {
    Provider = provider;
  }

  /// <summary>
  /// The name of the external login provider that failed.
  /// </summary>
  public string Provider { get; }
}