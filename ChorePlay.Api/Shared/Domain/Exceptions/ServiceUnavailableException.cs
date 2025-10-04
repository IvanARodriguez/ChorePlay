namespace ChorePlay.Api.Shared.Domain.Exceptions;

/// <summary>
/// Exception thrown when a service is temporarily unavailable.
/// </summary>
public class ServiceUnavailableException : Exception
{
  /// <summary>
  /// Initializes a new instance with default message.
  /// </summary>
  public ServiceUnavailableException()
      : base("The service is temporarily unavailable. Please try again later.")
  {
  }

  /// <summary>
  /// Initializes a new instance with a custom message.
  /// </summary>
  public ServiceUnavailableException(string message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance with service name and reason.
  /// </summary>
  /// <param name="serviceName">The name of the unavailable service.</param>
  /// <param name="reason">The reason the service is unavailable.</param>
  public ServiceUnavailableException(string serviceName, string reason)
      : base($"The '{serviceName}' service is temporarily unavailable: {reason}")
  {
    ServiceName = serviceName;
    Reason = reason;
  }

  /// <summary>
  /// Initializes a new instance with message and inner exception.
  /// </summary>
  public ServiceUnavailableException(string message, Exception innerException)
      : base(message, innerException)
  {
  }

  /// <summary>
  /// The name of the service that is unavailable.
  /// </summary>
  public string? ServiceName { get; }

  /// <summary>
  /// The reason the service is unavailable.
  /// </summary>
  public string? Reason { get; }
}
