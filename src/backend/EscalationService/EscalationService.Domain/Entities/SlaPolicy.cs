using EscalationService.Domain.Common;
using EscalationService.Domain.Enums;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Корневой агрегат политики SLA (Service Level Agreement).
/// Определяет требования к времени ответа и решения для эскалаций.
/// </summary>
public sealed class SlaPolicy : BaseEntity
{
    /// <summary>
    /// Название политики SLA.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Описание политики.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Минимальный уровень приоритета, к которому применяется данная политика.
    /// </summary>
    public EscalationPriority MinPriorityLevel { get; set; }

    /// <summary>
    /// Флаг активности политики.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Требуемое время ответа в минутах.
    /// </summary>
    public int ResponseTimeMinutes { get; set; }

    /// <summary>
    /// Требуемое время решения в минутах.
    /// </summary>
    public int ResolutionTimeMinutes { get; set; }
}