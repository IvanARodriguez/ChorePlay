using FluentValidation;

namespace ChorePlay.Api.Features.Auth.GoogleLogin;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
  public GoogleLoginRequestValidator()
  {
    RuleFor(x => x.IdToken)
    .NotEmpty().WithMessage("Google IdToken is required");
  }
}