using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.SlaPolicy;

public sealed record UpdateSlaPolicyRequest(
    string Name,
    string? Description,
    EscalationPriority MinPriorityLevel,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes);
