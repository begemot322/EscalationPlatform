using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Ответ с полными данными эскалации
/// </summary>
public sealed record EscalationResponse(
    long Id,
    string Name,
    string? Description,
    EscalationStatus Status,
    EscalationPriority Priority,
    EscalationType Type,
    Guid AuthorId,
    long SlaPolicyId,
    string SlaPolicyName,
    long ProjectId,
    string ProjectName,
    DateTimeOffset? FirstResponseAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<EscalationParticipantResponse> Participants,
    IReadOnlyList<CriterionResponse> Criteria,
    IReadOnlyList<CommentResponse> Comments,
    EscalationMetricsResponse? Metrics,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);