using EscalationService.Application.DTOs.Project;
using EscalationService.Domain.Entities;

namespace EscalationService.Application.Mappings;

public static class ProjectMapper
{
    public static ProjectResponse ToResponse(Project p) => new(
        p.Id, p.Name, p.Description, p.Code,
        p.IsActive, p.ArchivedAt, p.CreatedAt, p.UpdatedAt,
        p.Owners.Select(o => new ProjectOwnerResponse(o.UserId, o.Role, o.AssignedAt)).ToList());
}
