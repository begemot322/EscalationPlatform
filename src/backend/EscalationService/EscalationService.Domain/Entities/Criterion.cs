using EscalationService.Domain.Common;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Критерий выполнения эскалации.
/// Используется для отслеживания прогресса по чек-листу требований.
/// </summary>
public sealed class Criterion : BaseEntity
{
    /// <summary>
    /// Название критерия.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Описание критерия.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Порядок отображения критерия в списке.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Флаг выполнения критерия.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Дата и время выполнения критерия.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }
    
    /// <summary>
    /// Идентификатор эскалации, к которой относится критерий.
    /// </summary>
    public long EscalationId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Эскалация.
    /// </summary>
    public Escalation Escalation { get; set; } = null!;
}