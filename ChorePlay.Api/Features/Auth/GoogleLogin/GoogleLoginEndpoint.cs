using System.Security.Claims;
using ChorePlay.Api.Shared.Auth;
using ChorePlay.Api.Shared.Security;
using Google.Apis.Auth;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public static class GoogleLoginEndpoints
{
    public static IEndpointRouteBuilder MapGoogleLogin(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/auth/google/login",
            (
                [FromQuery] string redirectUrl,
                LinkGenerator linkGenerator,
                SignInManager<AppUser> signManager,
                HttpContext httpContext,
                ILogger<Program> logger,
                IConfiguration config
            ) =>
            {
                var callbackUrl = linkGenerator.GetPathByName(httpContext, "GoogleLoginCallback");

                var uiClient = config["UIClients:Web"];
                var redirect = string.IsNullOrWhiteSpace(redirectUrl)
                    ? uiClient
                    : redirectUrl.TrimStart('/');

                var properties = signManager.ConfigureExternalAuthenticationProperties(
                    "Google",
                    $"{callbackUrl}?redirectUrl={uiClient}/{redirect}"
                );

                return Results.Challenge(properties, ["Google"]);
            }
        );

        _ = endpoints
            .MapGet(
                "/api/auth/google/callback",
                static async (
                    [FromQuery] string redirectUrl,
                    HttpContext context,
                    IMediator mediator,
                    CookieManager cookieManager,
                    ILogger<Program> logger
                ) =>
                {
                    var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
                    if (!result.Succeeded || result.Principal == null)
                    {
                        return Results.Unauthorized();
                    }
                    foreach (var claim in result.Principal.Claims)
                    {
                        logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                    var payload = new GoogleJsonWebSignature.Payload
                    {
                        Email = result.Principal.FindFirstValue(ClaimTypes.Email),
                        Name = result.Principal.FindFirstValue(ClaimTypes.Name),
                        Picture = result.Principal.FindFirstValue("picture"),
                        GivenName = result.Principal.FindFirstValue(ClaimTypes.GivenName),
                        FamilyName = result.Principal.FindFirstValue(ClaimTypes.Surname),
                    };

                    var response = await mediator.Send(
                        new GoogleLoginCommand(payload),
                        context.RequestAborted
                    );

                    if (!string.IsNullOrEmpty(response.RefreshToken))
                        cookieManager.SetRefreshToken(
                            response.RefreshToken,
                            response.RefreshTokenExpiresAtUtc
                        );

                    // explicitly clearing the temporary external login
                    // cookie once it has served its purpose.
                    await context.SignOutAsync(IdentityConstants.ExternalScheme);

                    return Results.Redirect(redirectUrl);
                }
            )
            .WithName("GoogleLoginCallback");

        return endpoints;
    }
}
