using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.SlaPolicy;

public sealed record SlaPolicyResponse(
    long Id,
    string Name,
    string? Description,
    string MinPriorityLevel,
    bool IsActive,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
