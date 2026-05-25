using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Запрос на создание новой эскалации
/// </summary>
/// <param name="Name">Название эскалации</param>
/// <param name="Description">Описание проблемы (необязательно)</param>
/// <param name="Priority">Приоритет эскалации</param>
/// <param name="Type">Тип эскалации</param>
/// <param name="AuthorId">Идентификатор автора</param>
/// <param name="SlaPolicyId">Идентификатор SLA политики</param>
/// <param name="ProjectId">Идентификатор проекта</param>
/// <param name="Participants">Список участников (необязательно)</param>
/// <param name="Criteria">Список критериев срабатывания (необязательно)</param>
public sealed record CreateEscalationRequest(
    string Name,
    string? Description,
    EscalationPriority Priority,
    EscalationType Type,
    Guid AuthorId,
    long SlaPolicyId,
    long ProjectId,
    IReadOnlyList<EscalationParticipantRequest>? Participants,
    IReadOnlyList<CriterionRequest>? Criteria);
