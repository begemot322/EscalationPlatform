using EscalationPlatform.Bus.RabbitMq;
using EscalationPlatform.OpenTelemetry;
using EscalationPlatform.Shared.Messages;
using Microsoft.EntityFrameworkCore;
using UserService.Application;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Infrastructure;
using UserService.Infrastructure.Persistence;

namespace UserService.Api;

/// <summary>
/// Класс регистрации зависимостей API слоя для сервиса пользователей
/// </summary>
public static class DependencyInjection
{
    private const string ServiceName = "EscalationPlatformUserService";

    /// <summary>
    /// Настраивает и регистрирует все сервисы UserService
    /// </summary>
    public static WebApplicationBuilder AddUserServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var openTelemetryEndpoint = configuration.GetConnectionString(OpenTelemetryServiceExtensions.ConnectionStringName);
        var resourceBuilder = TelemetryResourceBuilder.CreateResource(ServiceName, out var version);

        Console.WriteLine($"Starting {ServiceName}:{version}\n");

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Logging.AddLogging(resourceBuilder, openTelemetryEndpoint);
        
        // Конфигурация RabbitMq
        var rabbitMqConnectionString = configuration.GetConnectionString(BusServiceExtensions.ConnectionStringName);
        var busConfiguration = configuration
            .GetSection(BusServiceExtensions.BusConfigurationSectionName)
            .Get<BusConfiguration>() ?? new BusConfiguration();

        builder.Services
            .AddTelemetry(resourceBuilder, openTelemetryEndpoint)
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddRabbitMq(
                connectionString: rabbitMqConnectionString,
                configuration: busConfiguration,
                configureRabbit: (_, cfg, config) =>
                {
                    cfg.ConfigureMessage<OtpCodeRequested>(Exchanges.UserEmailOtpRequested, config);
                });

        return builder;
    }

    public static async Task InitializeAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await db.Database.MigrateAsync();
        logger.LogInformation("UserService database migrated successfully");
        await DataSeeder.SeedAsync(db, hasher, logger);
    }
}
