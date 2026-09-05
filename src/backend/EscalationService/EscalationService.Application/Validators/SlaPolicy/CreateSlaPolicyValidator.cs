using EscalationService.Application.DTOs.SlaPolicy;
using FluentValidation;

namespace EscalationService.Application.Validators.SlaPolicy;

public sealed class CreateSlaPolicyValidator : AbstractValidator<CreateSlaPolicyRequest>
{
    public CreateSlaPolicyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MinPriorityLevel)
            .IsInEnum();

        RuleFor(x => x.ResponseTimeMinutes)
            .GreaterThan(0);

        RuleFor(x => x.ResolutionTimeMinutes)
            .GreaterThan(0)
            .GreaterThanOrEqualTo(x => x.ResponseTimeMinutes)
            .WithMessage("Resolution time must be greater than or equal to response time");
    }
}
