using EscalationPlatform.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Application;
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
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT токен"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        builder.Logging.AddLogging(resourceBuilder, openTelemetryEndpoint);

        builder.Services
            .AddTelemetry(resourceBuilder, openTelemetryEndpoint)
            .AddApplication()
            .AddInfrastructure(configuration);

        return builder;
    }

    public static async Task InitializeAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await db.Database.MigrateAsync();
        logger.LogInformation("UserService database migrated successfully");
    }
}
