using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Services.Implementation;
using UserService.Application.Services.Interfaces;
using UserService.Application.Validators;

namespace UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IUserService, UserService.Application.Services.Implementation.UserService>();

        services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();

        return services;
    }
}
