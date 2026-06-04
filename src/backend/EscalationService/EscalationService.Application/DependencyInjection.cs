using EscalationService.Application.Services.Implementation;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Application.Validators.SlaPolicy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EscalationService.Application;

/// <summary>
/// Класс регистрации зависимостей Application слоя для сервиса эскалаций
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует все сервисы Application слоя в контейнере зависимостей
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();
        
        // Зарегал сразу все валидаторы через 1
        services.AddValidatorsFromAssemblyContaining<CreateSlaPolicyValidator>();

        return services;
    }

    /// <summary>
    /// Регистрирует сервисы бизнес-логики приложения
    /// </summary>
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IExcelReaderService, ExcelReaderService>();
        services.AddScoped<ISlaPolicyService, SlaPolicyService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IEscalationService, Services.Implementation.EscalationService>();
        
        return services;
    }
}
