namespace EscalationService.Domain.Enums;

/// <summary>
/// Статус жизненного цикла эскалации
/// </summary>
public enum EscalationStatus
{
    /// <summary>Новая эскалация, создана, но еще не взята в работу.</summary>
    New = 1,

    /// <summary>Эскалация находится в процессе обработки.</summary>
    InProgress = 2,

    /// <summary>Ожидание ответа от пользователя или внешней системы.</summary>
    WaitingForResponse = 3,

    /// <summary>Эскалация решена, ожидает подтверждения закрытия.</summary>
    Resolved = 4,

    /// <summary>Эскалация успешно закрыта.</summary>
    Closed = 5,

    /// <summary>Эскалация отменена.</summary>
    Cancelled = 6
}