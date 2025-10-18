using ChorePlay.Api.Features.Auth.Register;
using ChorePlay.Api.Shared.Domain.Exceptions;
using ChorePlay.Api.Shared.Security;
using Mediator;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ChorePlay.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/auth/login",
            static async (
                LoginRequest? request,
                IMediator mediator,
                ILogger<Program> logger,
                CookieManager cookieManager
            ) =>
            {
                if (request is null)
                    throw new BadRequestException("Invalid Request Body");

                var result = await mediator.Send(new LoginCommand(request));

                cookieManager.SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

                return Results.Ok(
                    new
                    {
                        AccessToken = result.AccessToken,
                        ExpirationDate = result.AccessTokenExpiresAtUtc,
                    }
                );
            }
        );

        return endpoints;
    }
}
