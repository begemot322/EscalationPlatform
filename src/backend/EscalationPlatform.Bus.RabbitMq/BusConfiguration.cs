using EscalationPlatform.Bus.RabbitMq.Common;
using JetBrains.Annotations;

namespace EscalationPlatform.Bus.RabbitMq;

/// <summary>
/// Настройки конфигурации MassTransit
/// </summary>
[PublicAPI]
public sealed class BusConfiguration
{
    /// <summary>
    /// Настройки для консюмеров
    /// </summary>
    public Dictionary<string, TopologyConfiguration> Topology { get; init; } = new();
}