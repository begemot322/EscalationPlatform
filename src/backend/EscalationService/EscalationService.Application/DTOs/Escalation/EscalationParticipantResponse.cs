using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Ответ с данными участника эскалации
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Role">Роль участника в эскалации</param>
/// <param name="AssignedAt">Дата и время назначения участника</param>
public sealed record EscalationParticipantResponse(
    long UserId,
    ParticipantRole Role,
    DateTimeOffset AssignedAt);
