using EscalationPlatform.OpenTelemetry;
using EscalationService.Application;
using EscalationService.Infrastructure;
using EscalationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscalationService.Api;

/// <summary>
/// Класс регистрации зависимостей API слоя для сервиса эскалаций
/// </summary>
public static class DependencyInjection
{
    private const string ServiceName = "EscalationPlatformEscalationService";

    /// <summary>
    /// Настраивает и регистрирует все сервисы EscalationService
    /// </summary>
    public static WebApplicationBuilder AddEscalationServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        
        var openTelemetryEndpoint = configuration.GetConnectionString(OpenTelemetryServiceExtensions.ConnectionStringName);
        var resourceBuilder = TelemetryResourceBuilder.CreateResource(ServiceName, out var version);
        
        Console.WriteLine($"Starting {ServiceName}:{version}\n");
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Logging.AddLogging(resourceBuilder, openTelemetryEndpoint);
        
        builder.Services
            .AddTelemetry(resourceBuilder, openTelemetryEndpoint)
            .AddApplication()
            .AddInfrastructure(configuration);

        return builder;
    }

    /// <summary>
    /// Инициализирует базу данных EscalationService (выполняет миграции и сидирование)
    /// </summary>
    public static async Task InitializeAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EscalationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully");
        await DataSeeder.SeedAsync(db, logger);
    }
}
