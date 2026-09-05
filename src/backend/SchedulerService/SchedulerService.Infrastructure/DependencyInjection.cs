using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SchedulerService.Clients;

namespace SchedulerService;

/// <summary>
/// Регистрация HTTP-клиентов Infrastructure слоя.
/// </summary>
public static class DependencyInjection
{
    private const int RetryCount = 3;
    private const int TimeoutSeconds = 10;
    private const int MedianDelayMillis = 500;

    private const string EscalationServiceUrlKey = "Services:EscalationService";
    private const string UserServiceUrlKey = "Services:UserService";
    private const string EscalationServiceUrlDefault = "http://localhost:5020/";
    private const string UserServiceUrlDefault = "http://localhost:5219/";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var escalationBase = configuration[EscalationServiceUrlKey] ?? EscalationServiceUrlDefault;
        var userBase = configuration[UserServiceUrlKey] ?? UserServiceUrlDefault;

        services
            .AddHttpClient<IEscalationServiceClient, EscalationServiceClient>(client =>
            {
                client.BaseAddress = new Uri(escalationBase);
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            })
            .AddPolicyHandler((sp, _) => GetRetryPolicy(
                sp.GetRequiredService<ILogger<EscalationServiceClient>>(),
                nameof(EscalationServiceClient)));

        services
            .AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(userBase);
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            })
            .AddPolicyHandler((sp, _) => GetRetryPolicy(
                sp.GetRequiredService<ILogger<UserServiceClient>>(),
                nameof(UserServiceClient)));

        return services;
    }

    /// <summary>
    /// Retry-политика: 3 попытки с DecorrelatedJitterBackoffV2.
    /// Повторяет при сетевых ошибках и HTTP 5xx / 408.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        ILogger logger,
        string clientName)
    {
        var delays = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(MedianDelayMillis),
            retryCount: RetryCount);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                sleepDurations: delays,
                onRetry: (outcome, delay, attempt, _) =>
                    logger.LogWarning(
                        "[{Client}] Retry {Attempt}/{Max} after {Delay:F0}ms — {Reason}",
                        clientName, attempt, RetryCount, delay.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()));
    }
}
