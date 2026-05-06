using EscalationService.Domain.Enums;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Участник эскалации.
/// Связывает пользователя с эскалацией и определяет его роль в контексте данной эскалации. (many to many)
/// </summary>
public sealed class EscalationParticipant
{
    /// <summary>
    /// Идентификатор пользователя из внешнего сервиса.
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Идентификатор эскалации.
    /// </summary>
    public long EscalationId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Эскалация.
    /// </summary>
    public Escalation Escalation { get; set; } = null!;
    
    /// <summary>
    /// Роль участника в эскалации.
    /// </summary>
    public ParticipantRole Role { get; set; }
    
    /// <summary>
    /// Дата и время назначения участника.
    /// </summary>
    public DateTimeOffset AssignedAt { get; set; }
}