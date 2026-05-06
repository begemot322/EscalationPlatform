namespace EscalationService.Domain.Entities;

/// <summary>
/// Метрики эскалации. (1 to 1)
/// </summary>
public sealed class EscalationMetrics
{
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
    /// Время первого ответа в минутах.
    /// </summary>
    public int FirstResponseTimeMinutes { get; set; }

    /// <summary>
    /// Время разрешения в минутах.
    /// </summary>
    public int ResolutionTimeMinutes { get; set; }

    /// <summary>
    /// Время до закрытия в минутах.
    /// </summary>
    public int TimeToCloseMinutes { get; set; }

    /// <summary>
    /// Флаг нарушения SLA.
    /// </summary>
    public bool SlaViolated { get; set; }

    /// <summary>
    /// Общее количество комментариев.
    /// </summary>
    public int TotalComments { get; set; }

    /// <summary>
    /// Общее количество участников.
    /// </summary>
    public int TotalParticipants { get;  set; }
}