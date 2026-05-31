using EscalationPlatform.Bus.RabbitMq;
using EscalationPlatform.OpenTelemetry;
using EscalationPlatform.Shared.Messages;
using OpenTelemetry.Resources;
using SchedulerService.Configuration;
using SchedulerService.Services;

namespace SchedulerService;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddSchedulerServices(
        this IHostApplicationBuilder builder,
        ResourceBuilder telemetryResource)
    {
        // Конфигурация планировщика
        builder.Services.Configure<SchedulerOptions>(
            builder.Configuration.GetSection(SchedulerOptions.SectionName));

        // HTTP-клиенты с retry (Infrastructure слой)
        builder.Services.AddInfrastructure(builder.Configuration);

        // RabbitMQ — только публикация, без consumers
        var rabbitMqConnectionString = builder.Configuration
            .GetConnectionString(BusServiceExtensions.ConnectionStringName);

        var busConfiguration = builder.Configuration
            .GetSection(BusServiceExtensions.BusConfigurationSectionName)
            .Get<BusConfiguration>() ?? new BusConfiguration();

        builder.Services.AddRabbitMq(
            connectionString: rabbitMqConnectionString,
            configuration: busConfiguration,
            configureRabbit: (_, cfg, config) =>
            {
                cfg.ConfigureMessage<SlaBreached>(Exchanges.EscalationSlaBreached, config);
                cfg.ConfigureMessage<MonthlyReportRequested>(Exchanges.EscalationMonthlyReport, config);
            });

        // Telemetry + Logging
        var otlpEndpoint = builder.Configuration
            .GetConnectionString(OpenTelemetryServiceExtensions.ConnectionStringName);

        builder.Logging.AddLogging(telemetryResource, otlpEndpoint);
        builder.Services.AddTelemetry(telemetryResource, otlpEndpoint);

        // BackgroundServices
        builder.Services.AddHostedService<SlaMonitorWorker>();
        builder.Services.AddHostedService<MonthlyReportWorker>();

        return builder;
    }
}