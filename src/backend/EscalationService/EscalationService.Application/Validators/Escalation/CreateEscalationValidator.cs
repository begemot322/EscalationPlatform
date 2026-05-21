using EscalationService.Application.DTOs.Escalation;
using FluentValidation;

namespace EscalationService.Application.Validators.Escalation;

public sealed class CreateEscalationValidator : AbstractValidator<CreateEscalationRequest>
{
    public CreateEscalationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.SlaPolicyId)
            .GreaterThan(0).WithMessage("SlaPolicyId must be a valid identifier.");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a valid identifier.");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("AuthorId must be a valid identifier.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority has an invalid value.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type has an invalid value.");
    }
}
