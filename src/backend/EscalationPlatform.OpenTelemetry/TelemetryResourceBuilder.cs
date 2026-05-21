using System.Reflection;
using OpenTelemetry.Resources;

namespace EscalationPlatform.OpenTelemetry;

/// <summary>
/// Содержит методы для создания информации о ресурсе
/// </summary>
public static class TelemetryResourceBuilder
{
    private const string FallbackVersion = "1.0.0";

    /// <summary>
    /// Возвращает объект, который содержит информацию о сервисе. Необходим для метрик и логов.
    /// </summary>
    /// <param name="serviceName">Название сервиса</param>
    /// <param name="version">Версия приложения</param>
    public static ResourceBuilder CreateResource(string serviceName, out string version)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        version = executingAssembly.GetName().Version?.ToString() ?? FallbackVersion;

        return ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceInstanceId: Environment.MachineName, serviceVersion: version);
    }
}