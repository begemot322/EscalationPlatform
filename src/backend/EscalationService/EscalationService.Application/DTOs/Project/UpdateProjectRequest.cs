namespace EscalationService.Application.DTOs.Project;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    IReadOnlyList<AddProjectOwnerRequest>? Owners);
