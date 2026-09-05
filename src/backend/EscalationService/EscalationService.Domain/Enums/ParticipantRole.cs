namespace EscalationService.Domain.Enums;

/// <summary>
/// Роль участника в контексте эскалации.
/// </summary>
public enum ParticipantRole
{
    /// <summary>Автор эскалации, инициатор запроса.</summary>
    Author = 1,

    /// <summary>Исполнитель, назначенный ответственным за решение.</summary>
    Assignee = 2,

    /// <summary>Наблюдатель, получает уведомления, но не участвует в решении.</summary>
    Observer = 3,

    /// <summary>Согласующий, утверждает решение или изменения.</summary>
    Approver = 4
}