using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Project;

public sealed record UpdateProjectOwnerRequest(ProjectRole Role);
