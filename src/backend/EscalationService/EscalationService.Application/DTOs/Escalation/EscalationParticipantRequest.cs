using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Запрос на добавление участника эскалации
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Role">Роль участника в эскалации</param>
public sealed record EscalationParticipantRequest(Guid UserId, ParticipantRole Role);
