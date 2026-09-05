using EscalationService.Application.DTOs.Escalation;
using FluentValidation;

namespace EscalationService.Application.Validators.Escalation;

public sealed class UpdateEscalationValidator : AbstractValidator<UpdateEscalationRequest>
{
    public UpdateEscalationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority has an invalid value.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status has an invalid value.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type has an invalid value.");

        RuleForEach(x => x.Criteria)
            .ChildRules(c => c
                .RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Criterion name cannot be empty.")
                .MaximumLength(300).WithMessage("Criterion name must not exceed 300 characters."));
    }
}
