using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Фильтр для запроса списка эскалаций
/// </summary>
public sealed record EscalationFilter(
    EscalationStatus? Status,
    EscalationPriority? Priority,
    EscalationType? Type,
    Guid? AuthorId,
    long? ProjectId,
    Guid? ResponsibleUserId,
    DateTimeOffset? CreatedAfter,
    DateTimeOffset? CreatedBefore);
