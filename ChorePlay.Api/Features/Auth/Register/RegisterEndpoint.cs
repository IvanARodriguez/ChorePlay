using ChorePlay.Api.Shared.Domain.Exceptions;
using Mediator;

namespace ChorePlay.Api.Features.Auth.Register
{
    public static class RegisterEndpoint
    {
        public static IEndpointRouteBuilder MapRegisterEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/auth/register", static async (
                RegisterRequest? request,
                IMediator mediator,
                ILogger<Program> logger

            ) =>
            {
                if (request is null)
                    throw new BadRequestException("Invalid Request Body");

                var response = await mediator.Send(new RegisterCommand(request));

                return Results.Ok(response);
            });

            return endpoints;
        }
    }
}