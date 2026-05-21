using EscalationPlatform.Bus.RabbitMq;
using EscalationPlatform.OpenTelemetry;
using MassTransit.Logging;
using MassTransit.Monitoring;
using NotificationService.Email;

namespace NotificationService.Host;

public static class HostStartup
{
    private const string ServiceName = "NotificationService";

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var telemetryResource = TelemetryResourceBuilder
            .CreateResource(ServiceName, out var version);

        // non empty console
        Console.WriteLine($"Starting {ServiceName}:{version}\n");

        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var rabbitMqConnectionString = hostContext.Configuration
                    .GetConnectionString(BusServiceExtensions.ConnectionStringName);
                
                var busConfiguration = hostContext.Configuration
                    .GetSection(BusServiceExtensions.BusConfigurationSectionName)
                    .Get<BusConfiguration>() ?? new BusConfiguration();

                services.Configure<SmtpOptions>(
                    hostContext.Configuration.GetSection(SmtpOptions.ConfigurationSectionName));
                
                services
                    .AddTelemetry(telemetryResource,
                        hostContext.Configuration.GetConnectionString("Otlp"),
                        tracingConfigure: cfg => cfg.AddSource(DiagnosticHeaders.DefaultListenerName),
                        metricsConfigure: cfg => cfg.AddMeter(InstrumentationOptions.MeterName))
                    .AddRabbitMq(
                        connectionString: rabbitMqConnectionString,
                        configuration: busConfiguration,
                        configureRabbit: (_, cfg, config) =>
                        {
                        });
                
                services.AddScoped<IEmailSender, MailKitSmtpService>();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.AddLogging(telemetryResource, hostContext.Configuration.GetConnectionString("Otlp"));
            });
    }
}