using System.Net;
using System.Text.Json;
using ChorePlay.Api.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ChorePlay.Api.Infrastructure.Middlewares;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<GlobalExceptionHandler> _logger = logger;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      await HandleExceptionAsync(context, ex);
    }
  }

  public async ValueTask<bool> HandleExceptionAsync(
      HttpContext httpContext,
      Exception exception)
  {
    LogException(exception, httpContext);

    var errorResponse = CreateErrorResponse(exception, httpContext);

    httpContext.Response.ContentType = "application/problem+json";
    httpContext.Response.StatusCode = errorResponse.Status ?? (int)HttpStatusCode.InternalServerError;

    await httpContext.Response.WriteAsync(
        JsonSerializer.Serialize(errorResponse, JsonOptions));

    return true;
  }

  private void LogException(Exception exception, HttpContext context)
  {
    var path = context.Request.Path;
    var user = context.User?.Identity?.Name ?? "Anonymous";
    var traceId = context.TraceIdentifier;

    switch (exception)
    {
      case NotFoundException:
      case ConflictException:
      case BadRequestException:
      case ValidationException:
      case UnauthorizedAccessException:
      case ForbiddenException:
      case BusinessRuleViolationException:
        _logger.LogWarning(exception,
            "[{TraceId}] {ExceptionType} at {Path} by {User}",
            traceId, exception.GetType().Name, path, user);
        break;
      case ServiceUnavailableException:
        _logger.LogError(exception,
            "[{TraceId}] {ExceptionType} at {Path} by {User}",
            traceId, exception.GetType().Name, path, user);
        break;

      case RateLimitExceededException:
        _logger.LogInformation(exception,
            "[{TraceId}] Rate Limit Exceeded at {Path} by {User}",
            traceId, path, user);
        break;

      default:
        _logger.LogError(exception,
            "[{TraceId}] Unhandled exception at {Path} by {User}: {Message}",
            traceId, path, user, exception.Message);
        break;
    }
  }

  private static ErrorResponse CreateErrorResponse(Exception exception, HttpContext context)
  {
    var traceId = context.TraceIdentifier;

    var response = exception switch
    {
      NotFoundException notFound => new ErrorResponse
      {
        Status = (int)HttpStatusCode.NotFound,
        Title = "Resource Not Found",
        Detail = notFound.Message,
        Type = Rfc.NotFound
      },

      BadRequestException badRequest => new ErrorResponse
      {
        Status = (int)HttpStatusCode.BadRequest,
        Title = "Resource Not Found",
        Detail = badRequest.Message,
        Type = Rfc.BadRequest
      },

      ValidationException validation => new ErrorResponse
      {
        Status = (int)HttpStatusCode.BadRequest,
        Title = "Validation Failed",
        Detail = validation.Message,
        Errors = validation.Errors,
        Type = Rfc.BadRequest
      },

      UnauthorizedAccessException => new ErrorResponse
      {
        Status = (int)HttpStatusCode.Unauthorized,
        Title = "Unauthorized",
        Detail = "You are not authorized to access this resource.",
        Type = Rfc.Unauthorized
      },

      InvalidOperationException invalidOp => new ErrorResponse
      {
        Status = (int)HttpStatusCode.BadRequest,
        Title = "Invalid Operation",
        Detail = invalidOp.Message,
        Type = Rfc.BadRequest
      },

      ConflictException conflict => new ErrorResponse
      {
        Status = (int)HttpStatusCode.Conflict,
        Title = "Conflict",
        Detail = conflict.Message,
        Type = Rfc.Conflict
      },

      _ => new ErrorResponse
      {
        Status = (int)HttpStatusCode.InternalServerError,
        Title = "Internal Server Error",
        Detail = "An unexpected error occurred. Please try again later.",
        Type = Rfc.ServerError
      }
    };

    // Add trace ID to the RFC 7807 extensions
    response.Extensions["traceId"] = traceId;
    return response;
  }

  public class ErrorResponse : ProblemDetails
  {
    public IDictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  }

  private static class Rfc
  {
    // 1xx Informational
    public const string Continue = "https://tools.ietf.org/html/rfc7231#section-6.2.1";               // 100 Continue
    public const string SwitchingProtocols = "https://tools.ietf.org/html/rfc7231#section-6.2.2";     // 101 Switching Protocols
    public const string Processing = "https://tools.ietf.org/html/rfc2518#section-10.1";              // 102 Processing

    // 2xx Success
    public const string OK = "https://tools.ietf.org/html/rfc7231#section-6.3.1";                      // 200 OK
    public const string Created = "https://tools.ietf.org/html/rfc7231#section-6.3.2";                 // 201 Created
    public const string Accepted = "https://tools.ietf.org/html/rfc7231#section-6.3.3";                // 202 Accepted
    public const string NoContent = "https://tools.ietf.org/html/rfc7231#section-6.3.5";               // 204 No Content
    public const string ResetContent = "https://tools.ietf.org/html/rfc7231#section-6.3.6";            // 205 Reset Content
    public const string PartialContent = "https://tools.ietf.org/html/rfc7233#section-4.1";            // 206 Partial Content

    // 3xx Redirection
    public const string MultipleChoices = "https://tools.ietf.org/html/rfc7231#section-6.4.1";         // 300 Multiple Choices
    public const string MovedPermanently = "https://tools.ietf.org/html/rfc7231#section-6.4.2";        // 301 Moved Permanently
    public const string Found = "https://tools.ietf.org/html/rfc7231#section-6.4.3";                    // 302 Found
    public const string SeeOther = "https://tools.ietf.org/html/rfc7231#section-6.4.4";                 // 303 See Other
    public const string NotModified = "https://tools.ietf.org/html/rfc7232#section-4.1";                // 304 Not Modified
    public const string TemporaryRedirect = "https://tools.ietf.org/html/rfc7231#section-6.4.7";       // 307 Temporary Redirect
    public const string PermanentRedirect = "https://tools.ietf.org/html/rfc7538#section-3";           // 308 Permanent Redirect

    // 4xx Client Errors
    public const string BadRequest = "https://tools.ietf.org/html/rfc7231#section-6.5.1";               // 400 Bad Request
    public const string Unauthorized = "https://tools.ietf.org/html/rfc7235#section-3.1";               // 401 Unauthorized
    public const string PaymentRequired = "https://tools.ietf.org/html/rfc7231#section-6.5.2";         // 402 Payment Required
    public const string Forbidden = "https://tools.ietf.org/html/rfc7231#section-6.5.3";                // 403 Forbidden
    public const string NotFound = "https://tools.ietf.org/html/rfc7231#section-6.5.4";                 // 404 Not Found
    public const string MethodNotAllowed = "https://tools.ietf.org/html/rfc7231#section-6.5.5";         // 405 Method Not Allowed
    public const string NotAcceptable = "https://tools.ietf.org/html/rfc7231#section-6.5.6";            // 406 Not Acceptable
    public const string ProxyAuthenticationRequired = "https://tools.ietf.org/html/rfc7235#section-3.2"; // 407 Proxy Authentication Required
    public const string RequestTimeout = "https://tools.ietf.org/html/rfc7231#section-6.5.7";           // 408 Request Timeout
    public const string Conflict = "https://tools.ietf.org/html/rfc7231#section-6.5.8";                 // 409 Conflict
    public const string Gone = "https://tools.ietf.org/html/rfc7231#section-6.5.9";                     // 410 Gone
    public const string LengthRequired = "https://tools.ietf.org/html/rfc7231#section-6.5.10";          // 411 Length Required
    public const string PreconditionFailed = "https://tools.ietf.org/html/rfc7232#section-4.2";         // 412 Precondition Failed
    public const string PayloadTooLarge = "https://tools.ietf.org/html/rfc7231#section-6.5.11";         // 413 Payload Too Large
    public const string URITooLong = "https://tools.ietf.org/html/rfc7231#section-6.5.12";              // 414 URI Too Long
    public const string UnsupportedMediaType = "https://tools.ietf.org/html/rfc7231#section-6.5.13";    // 415 Unsupported Media Type
    public const string RangeNotSatisfiable = "https://tools.ietf.org/html/rfc7233#section-4.4";        // 416 Range Not Satisfiable
    public const string ExpectationFailed = "https://tools.ietf.org/html/rfc7231#section-6.5.14";       // 417 Expectation Failed
    public const string ImATeapot = "https://tools.ietf.org/html/rfc2324#section-2.3.2";                 // 418 I'm a teapot
    public const string MisdirectedRequest = "https://tools.ietf.org/html/rfc7540#section-9.1.2";       // 421 Misdirected Request
    public const string UnprocessableEntity = "https://tools.ietf.org/html/rfc4918#section-11.2";       // 422 Unprocessable Entity
    public const string Locked = "https://tools.ietf.org/html/rfc4918#section-11.3";                     // 423 Locked
    public const string FailedDependency = "https://tools.ietf.org/html/rfc4918#section-11.4";          // 424 Failed Dependency
    public const string TooEarly = "https://tools.ietf.org/html/rfc8470#section-5.2";                    // 425 Too Early
    public const string UpgradeRequired = "https://tools.ietf.org/html/rfc7231#section-6.5.15";         // 426 Upgrade Required
    public const string PreconditionRequired = "https://tools.ietf.org/html/rfc6585#section-3";         // 428 Precondition Required
    public const string TooManyRequests = "https://tools.ietf.org/html/rfc6585#section-4";              // 429 Too Many Requests
    public const string RequestHeaderFieldsTooLarge = "https://tools.ietf.org/html/rfc6585#section-5";  // 431 Request Header Fields Too Large

    // 5xx Server Errors
    public const string ServerError = "https://tools.ietf.org/html/rfc7231#section-6.6.1";               // 500 Internal Server Error
    public const string NotImplemented = "https://tools.ietf.org/html/rfc7231#section-6.6.2";           // 501 Not Implemented
    public const string BadGateway = "https://tools.ietf.org/html/rfc7231#section-6.6.3";               // 502 Bad Gateway
    public const string ServiceUnavailable = "https://tools.ietf.org/html/rfc7231#section-6.6.4";       // 503 Service Unavailable
    public const string GatewayTimeout = "https://tools.ietf.org/html/rfc7231#section-6.6.5";           // 504 Gateway Timeout
    public const string HTTPVersionNotSupported = "https://tools.ietf.org/html/rfc7231#section-6.6.6"; // 505 HTTP Version Not Supported
    public const string VariantAlsoNegotiates = "https://tools.ietf.org/html/rfc2295#section-8.1";      // 506 Variant Also Negotiates
    public const string InsufficientStorage = "https://tools.ietf.org/html/rfc4918#section-11.5";      // 507 Insufficient Storage
    public const string LoopDetected = "https://tools.ietf.org/html/rfc5842#section-7.2";               // 508 Loop Detected
    public const string NotExtended = "https://tools.ietf.org/html/rfc2774#section-7";                  // 510 Not Extended
    public const string NetworkAuthenticationRequired = "https://tools.ietf.org/html/rfc6585#section-6"; // 511 Network Authentication Required
  }

}
