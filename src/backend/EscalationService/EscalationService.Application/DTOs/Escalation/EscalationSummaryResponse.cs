using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Краткий ответ с основными данными эскалации (для списков)
/// </summary>
public sealed record EscalationSummaryResponse(
    long Id,
    string Name,
    EscalationStatus Status,
    EscalationPriority Priority,
    EscalationType Type,
    long AuthorId,
    long SlaPolicyId,
    string SlaPolicyName,
    long ProjectId,
    string ProjectName,
    DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
