using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace ChorePlay.Api.Features.Auth.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName).NotEmpty().WithMessage("First name is required");
        RuleFor(r => r.LastName).NotEmpty().WithMessage("Last name is required");
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.Password).NotEmpty().MinimumLength(8);
    }
}
