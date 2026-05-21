using EscalationPlatform.Database.PgSql;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Infrastructure.Persistence;
using EscalationService.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace EscalationService.Infrastructure;

/// <summary>
/// Регистрация зависимостей слоя Infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Общий метод регистрации сервисов Infrastructure слоя
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration);

        return services;
    }

    /// <summary>
    /// Регистрирует сервисы для работы с базой данных
    /// </summary>
    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseServiceExtensions.ConnectionStringName)
                               ?? throw new InvalidOperationException(
                                   $"Connection string '{DatabaseServiceExtensions.ConnectionStringName}' is required");
        
        var dbConfig = configuration
            .GetSection(DatabaseServiceExtensions.ConfigurationSectionName)
            .Get<DatabaseConfiguration>() ?? new DatabaseConfiguration();
        
        services.AddSingleton<AuditSaveChangesInterceptor>();

        services.AddPostgres<EscalationDbContext>(
            connectionString,
            typeof(EscalationDbContext).Assembly.FullName!,
            dbConfig,
            configureDbContext: (sp, options) =>
                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped<IEscalationDbContext>(sp => sp.GetRequiredService<EscalationDbContext>());

        return services;
    }
}
