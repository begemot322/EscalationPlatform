using System.Security.Claims;
using System.Text;
using EscalationPlatform.Database.PgSql;
using EscalationPlatform.Shared.Options;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Infrastructure.Clients;
using EscalationService.Infrastructure.Persistence;
using EscalationService.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
            .AddPersistence(configuration)
            .AddAuth(configuration)
            .AddHttpClients(configuration);

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
    
    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var userServiceUrl = configuration["Services:UserService"] ?? "http://localhost:5219/";

        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            client.BaseAddress = new Uri(userServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
