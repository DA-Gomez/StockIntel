using FluentValidation;

namespace StockIntel.Application.Users.Register;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
  public RegisterUserValidator()
  {
    RuleFor(c => c.Email)
    .NotEmpty().WithMessage("Email is required.")
    .EmailAddress().WithMessage("Email must be a valid email address.")
    .MaximumLength(256);

    RuleFor(c => c.Password)
    .NotEmpty().WithMessage("Password is required.")
    .MinimumLength(8).WithMessage("Email must be at least 8 characters.")
    .MaximumLength(128);
  }
}