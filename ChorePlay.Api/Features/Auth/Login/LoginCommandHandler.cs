using ChorePlay.Api.Shared.Domain.Services;
using FluentValidation;
using Mediator;

namespace ChorePlay.Api.Features.Auth.Login;

public class LoginCommandHandler(AccountService accountService, IValidator<LoginRequest> validator)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<LoginResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        // Validate the request
        var validationResult = await validator.ValidateAsync(command.Request, ct);

        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Shared.Domain.Exceptions.ValidationException(errors);
        }

        // use accountService to login
        var result = await accountService.LoginAsync(command.Request, ct);

        return new LoginResponse(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc
        );
    }
}
