using EscalationPlatform.Bus.RabbitMq;
using EscalationPlatform.OpenTelemetry;
using NotificationService.Email;
using NotificationService.Email.Consumers;

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
                    .AddTelemetry(telemetryResource, hostContext.Configuration.GetConnectionString("Otlp"))
                    .AddRabbitMq(
                        connectionString: rabbitMqConnectionString,
                        configuration: busConfiguration,
                        configureConsumers: mt =>
                        {
                            mt.AddConsumer<OtpCodeRequestedConsumer>();
                            mt.AddConsumer<SlaBreachedConsumer>();
                        },
                        configureRabbit: (context, cfg, config) =>
                        {
                            cfg.ConfigureConsumer<OtpCodeRequestedConsumer>(
                                context, 
                                exchangeName: Exchanges.UserEmailOtpRequested, 
                                config);
                            cfg.ConfigureConsumer<SlaBreachedConsumer>(
                                context,
                                exchangeName: Exchanges.EscalationSlaBreached,
                                config);
                        });
                
                services.AddScoped<IEmailSender, MailKitSmtpService>();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.AddLogging(telemetryResource, hostContext.Configuration.GetConnectionString("Otlp"));
            });
    }
}