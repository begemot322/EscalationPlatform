namespace EscalationService.Domain.Enums;

/// <summary>
/// Приоритет эскалации, влияющий на SLA и порядок обработки.
/// </summary>
public enum EscalationPriority
{
    /// <summary>Низкий приоритет.</summary>
    Low = 1,

    /// <summary>Средний приоритет.</summary>
    Medium = 2,

    /// <summary>Высокий приоритет.</summary>
    High = 3,

    /// <summary>Критический приоритет, требует немедленного внимания.</summary>
    Critical = 4,

    /// <summary>Блокирующий приоритет, останавливает работу системы или процесса.</summary>
    Blocker = 5
}