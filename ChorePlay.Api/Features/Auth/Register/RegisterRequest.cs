using System.Text.Json;

namespace ChorePlay.Api.Features.Auth.Register;

public record RegisterRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async ValueTask<RegisterRequest?> BindAsync(HttpContext context)
    {
        if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
            return null;


        try
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return null;

            var request = JsonSerializer.Deserialize<RegisterRequest>(body, JsonOptions);

            return request;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
