using FluentValidation;
using UserService.Application.DTOs;

namespace UserService.Application.Validators;

public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
{
    public VerifyOtpDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$").WithMessage("'Code' must be exactly 6 digits.");
    }
}
