using JetBrains.Annotations;

namespace EscalationPlatform.Bus.RabbitMq.Common;

/// <summary>
/// Настройки параметров для ретраев 
/// </summary>
[PublicAPI]
public sealed class RetryParameters
{
    /// <summary>
    /// Максимальное количество попыток
    /// </summary>
    public int Limit { get; init; } = 3;
    
    /// <summary>
    /// Минимальный интервал времени
    /// </summary>
    public int MinInterval { get; init; } = 1;
    
    /// <summary>
    /// Максимальный интервал времени
    /// </summary>
    public int MaxInterval { get; init; } = 60;
    
    /// <summary>
    /// Дельта
    /// </summary>
    public int IntervalDelta { get; init; } = 5;
}