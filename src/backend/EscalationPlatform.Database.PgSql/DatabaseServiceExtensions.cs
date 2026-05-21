using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EscalationPlatform.Database.PgSql;

/// <summary>
/// Методы расширения для регистрации PostgreSQL
/// </summary>
public static class DatabaseServiceExtensions
{
     /// <summary>
    /// Имя строки подключения к базе данных
    /// </summary>
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// Имя секции конфигурации базы данных.
    /// </summary>
    public const string ConfigurationSectionName = "Database";

    /// <summary>
    /// Регистрирует базу данных в DI
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="migrationsAssembly">Ссылка на сборку, содержащую миграции Entity Framework</param>
    /// <param name="connectionString">Строка подключения к базе данных</param>
    /// <param name="databaseConfiguration">Конфигурация базы данных</param>
    /// <param name="configureDbContext">Дополнительная настройка контекста базы данных</param>
    /// <exception cref="ApplicationException">Выбрасывается, если строка подключения пуста или null.</exception>
    public static IServiceCollection AddPostgres<TDbContext>(this IServiceCollection services, string? connectionString,
        string migrationsAssembly, DatabaseConfiguration? databaseConfiguration = null,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureDbContext = null)  where TDbContext : DbContext
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        databaseConfiguration ??= new DatabaseConfiguration();
        
        services.TryAddSingleton(TimeProvider.System);
        
        services.AddDbContextPool<TDbContext>((sp, builder) =>
        {
            builder.UseNpgsql(connectionString,
                    npgsql =>
                    {
                        npgsql.MigrationsAssembly(migrationsAssembly);
                        
                        if (databaseConfiguration.ErrorRetryCount > 0)
                            npgsql.EnableRetryOnFailure(
                                maxRetryCount: databaseConfiguration.ErrorRetryCount,
                                maxRetryDelay: TimeSpan.FromSeconds(databaseConfiguration.ErrorRetryDelay),
                                errorCodesToAdd: null);

                        if (databaseConfiguration.CommandTimeout != null)
                            npgsql.CommandTimeout(databaseConfiguration.CommandTimeout);
                    })
                .UseSnakeCaseNamingConvention();
            
            configureDbContext?.Invoke(sp, builder);

            if (!databaseConfiguration.LogSensitiveData) return;
            builder.EnableSensitiveDataLogging();
            builder.EnableDetailedErrors();
            builder.LogTo(Console.WriteLine, LogLevel.Information);
        }, poolSize: databaseConfiguration.PoolSize);

        return services;
    }
}