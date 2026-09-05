using EscalationService.Application.DTOs.Project;
using FluentValidation;

namespace EscalationService.Application.Validators.Project;

public sealed class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
