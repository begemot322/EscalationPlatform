using JetBrains.Annotations;

namespace EscalationPlatform.Bus.RabbitMq.Common;

/// <summary>
/// Настройки для консюмера
/// </summary>
[PublicAPI]
public sealed class TopologyConfiguration
{
    /// <summary>
    /// Наименование exchange
    /// </summary>
    public string? ExchangeName { get; init; }
    
    /// <summary>
    /// Наименование очереди
    /// </summary>
    public string? QueueName { get; init; }

    /// <summary>
    /// Количество сообщение получаемое за один раз, по умолчанию 16
    /// </summary>
    public int? PrefetchCount { get; init; }
    
    /// <summary>
    /// Настройки ретраев при получении сообщения
    /// </summary>
    public RetryParameters? RetryPolicy { get; init; }

    /// <summary>
    /// Флаг создания очереди
    /// </summary>
    public bool CreateQueue { get; init; }
}