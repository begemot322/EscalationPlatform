using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EscalationPlatform.OpenTelemetry;

/// <summary>
/// Содержит методы для настройки логов и метрик 
/// </summary>
public static class OpenTelemetryServiceExtensions
{
    /// <summary>
    /// Имя строки подключения
    /// </summary>
    public const string ConnectionStringName = "Otlp";

    /// <summary>
    /// Настраивает лоигирование в консоль и коллектор OTLP 
    /// </summary>
    /// <param name="loggingBuilder">Интерфейс настройки логов</param>
    /// <param name="resourceBuilder">Информация о ресурсе для OpenTelemetry</param>
    /// <param name="connectionString">Строка подключения к коллектору OTLP</param>
    public static void AddLogging(this ILoggingBuilder loggingBuilder, ResourceBuilder resourceBuilder,
        string? connectionString)
    {
        loggingBuilder
            .ClearProviders()
            .AddConsole()
            .AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);

                // include traces
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;

                // add exporter
                if (string.IsNullOrEmpty(connectionString))
                    return;

                options.AddOtlpExporter(otlpOptions => { otlpOptions.Endpoint = new Uri(connectionString); });
            });
    }

    /// <summary>
    /// Добавляет сервисы для сбора метрик в формате OTLP
    /// </summary>
    /// <param name="services">Контейнер DI</param>
    /// <param name="resourceBuilder">Информация о ресурсе для OpenTelemetry</param>
    /// <param name="connectionString">Строка подключения к коллектору OTLP</param>
    /// <param name="tracingConfigure">Делегат для настройки трейсинга</param>
    /// <param name="metricsConfigure">Делегат для настройки метрик</param>
    public static IServiceCollection AddTelemetry(this IServiceCollection services, ResourceBuilder resourceBuilder,
        string? connectionString, Action<TracerProviderBuilder>? tracingConfigure = null,
        Action<MeterProviderBuilder>? metricsConfigure = null)
    {
        var openTelemetryBuilder = services.AddOpenTelemetry();
        var collectorEndpoint = connectionString != null ? new Uri(connectionString) : null;

        openTelemetryBuilder.WithTracing(tracingBuilder =>
        {
            tracingBuilder.SetResourceBuilder(resourceBuilder);
            
            tracingConfigure?.Invoke(tracingBuilder);

            if (collectorEndpoint == null)
                return;
            tracingBuilder.AddOtlpExporter(options => options.Endpoint = collectorEndpoint);
        });

        openTelemetryBuilder.WithMetrics(metricsBuilder =>
        {
            metricsBuilder.SetResourceBuilder(resourceBuilder)
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation();
            
            metricsConfigure?.Invoke(metricsBuilder);

            if (collectorEndpoint == null)
                return;
            metricsBuilder.AddOtlpExporter(options => options.Endpoint = collectorEndpoint);
        });

        return services;
    }
}