using EscalationService.Application.DTOs.Project;
using FluentValidation;

namespace EscalationService.Application.Validators.Project;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
