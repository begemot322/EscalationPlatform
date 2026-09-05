using System.Security.Claims;
using System.Text;
using EscalationPlatform.Database.PgSql;
using EscalationPlatform.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Interfaces.Identity;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure;

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
            .AddIdentityServices(configuration)
            .AddAuth(configuration);

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
        
        services.AddPostgres<UserDbContext>(
            connectionString,
            typeof(UserDbContext).Assembly.FullName!,
            dbConfig);

        services.AddScoped<IUserDbContext>(sp => sp.GetRequiredService<UserDbContext>());

        return services;
    }

    /// <summary>
    /// Регистрирует сервисы идентификации и аутентификации пользователей
    /// </summary>
    private static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    /// <summary>
    /// Регистрирует сервисы аутентификации и авторизации на основе JWT Bearer токенов
    /// </summary>
    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // отключаем переименование sub → NameIdentifier
                options.MapInboundClaims = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
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
