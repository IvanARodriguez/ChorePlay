using ChorePlay.Api.Shared.Domain.Exceptions;
using ChorePlay.Api.Shared.Domain.Services;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace ChorePlay.Api.Features.Auth.Register
{
    public class RegisterCommandHandler(AccountService accountService, IValidator<RegisterRequest> validator) : ICommandHandler<RegisterCommand, RegisterResponse>
    {
        public async ValueTask<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(command.Request, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Shared.Domain.Exceptions.ValidationException(errors);
            }
            var results = await accountService.RegisterAsync(command.Request, cancellationToken) ?? throw new ConflictException("user not returned after registration");

            return new RegisterResponse($"{results.FirstName} created successfully");
        }
    }
}