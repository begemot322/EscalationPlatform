using EscalationService.Application.DTOs.SlaPolicy;
using EscalationService.Domain.Entities;

namespace EscalationService.Application.Mappings;

public static class SlaPolicyMapper
{
    public static SlaPolicyResponse ToResponse(SlaPolicy p) => new(
        p.Id, p.Name, p.Description, p.MinPriorityLevel.ToString(),
        p.IsActive, p.ResponseTimeMinutes, p.ResolutionTimeMinutes,
        p.CreatedAt, p.UpdatedAt);
}
