using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

public sealed record UpdateEscalationRequest(
    string Name,
    string? Description,
    EscalationPriority Priority,
    EscalationStatus Status,
    EscalationType Type,
    DateTimeOffset? FirstResponseAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyList<EscalationParticipantRequest>? Participants,
    IReadOnlyList<CriterionUpdateItem>? Criteria);
