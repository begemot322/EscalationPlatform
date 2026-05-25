namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Данные эскалации для SLA-мониторинга.
/// </summary>
public sealed record SlaCheckItem(
    long Id,
    string Name,
    string ProjectName,
    string Priority,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SlaCheckParticipant> Assignees);

/// <summary>
/// Исполнитель эскалации для SLA-мониторинга.
/// </summary>
public sealed record SlaCheckParticipant(
    Guid UserId);
