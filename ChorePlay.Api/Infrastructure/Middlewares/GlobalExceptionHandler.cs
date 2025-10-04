using System.Net;
using System.Text.Json;
using ChorePlay.Api.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ChorePlay.Api.Infrastructure.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger = logger;

  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    // Log the exception with appropriate level
    LogException(exception, httpContext);

    // Create error response based on exception type
    var errorResponse = CreateErrorResponse(exception);

    // Set response details
    httpContext.Response.ContentType = "application/json";
    httpContext.Response.StatusCode = errorResponse.StatusCode;

    // Write response
    await httpContext.Response.WriteAsync(
        JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }),
        cancellationToken);

    return true;
  }

  private void LogException(Exception exception, HttpContext context)
  {
    var requestPath = context.Request.Path;
    var userId = context.User?.Identity?.Name ?? "Anonymous";

    switch (exception)
    {
      case NotFoundException:
      case ConflictException:
      case BadRequestException:
        _logger.LogWarning(exception,
            "{ExceptionType}: {Path} by {User}",
            exception.GetType().Name, requestPath, userId);
        break;

      case ValidationException:
        _logger.LogWarning(exception,
            "Validation Failed: {Path} by {User}",
            requestPath, userId);
        break;

      case UnauthorizedAccessException:
      case ForbiddenException:
        _logger.LogWarning(exception,
            "Authorization Failed: {Path} by {User}",
            requestPath, userId);
        break;

      case BusinessRuleViolationException:
        _logger.LogWarning(exception,
            "Business Rule Violated: {Path} by {User}",
            requestPath, userId);
        break;

      case RateLimitExceededException:
        _logger.LogInformation(exception,
            "Rate Limit Exceeded: {Path} by {User}",
            requestPath, userId);
        break;

      case ExternalLoginProviderException:
      case ServiceUnavailableException:
        _logger.LogError(exception,
            "{ExceptionType}: {Path} by {User}",
            exception.GetType().Name, requestPath, userId);
        break;

      default:
        _logger.LogError(exception,
            "Unhandled Exception: {Path} by {User}. Message: {Message}",
            requestPath, userId, exception.Message);
        break;
    }
  }

  private ErrorResponse CreateErrorResponse(Exception exception)
  {
    return exception switch
    {
      NotFoundException notFound => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.NotFound,
        Title = "Resource Not Found",
        Detail = notFound.Message,
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
      },

      ValidationException validation => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Title = "Validation Failed",
        Detail = validation.Message,
        Errors = validation.Errors,
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
      },

      UnauthorizedAccessException => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
        Title = "Unauthorized",
        Detail = "You are not authorized to access this resource.",
        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
      },

      ExternalLoginProviderException loginEx => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Title = "External Login Failed",
        Detail = loginEx.Message,
        Type = "about:blank"
      },

      InvalidOperationException invalidOp => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Title = "Invalid Operation",
        Detail = invalidOp.Message,
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
      },

      _ => new ErrorResponse
      {
        StatusCode = (int)HttpStatusCode.InternalServerError,
        Title = "Internal Server Error",
        Detail = "An unexpected error occurred. Please try again later.",
        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
      }
    };
  }
}

/// <summary>
/// Standard error response format following RFC 7807 Problem Details.
/// </summary>
public class ErrorResponse
{
  /// <summary>
  /// HTTP status code.
  /// </summary>
  public int StatusCode { get; set; }

  /// <summary>
  /// A short, human-readable summary of the problem type.
  /// </summary>
  public string Title { get; set; } = string.Empty;

  /// <summary>
  /// A human-readable explanation specific to this occurrence of the problem.
  /// </summary>
  public string Detail { get; set; } = string.Empty;

  /// <summary>
  /// A URI reference that identifies the problem type.
  /// </summary>
  public string Type { get; set; } = string.Empty;

  /// <summary>
  /// Validation errors (if applicable).
  /// </summary>
  public IDictionary<string, string[]>? Errors { get; set; }

  /// <summary>
  /// Timestamp when the error occurred.
  /// </summary>
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Trace identifier for debugging.
  /// </summary>
  public string? TraceId { get; set; }
}