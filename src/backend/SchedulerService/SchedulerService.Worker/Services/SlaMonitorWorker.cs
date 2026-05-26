using EscalationPlatform.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Options;
using SchedulerService.Clients;
using SchedulerService.Configuration;

namespace SchedulerService.Services;

/// <summary>
/// Фоновый Worker — каждый час проверяет активные эскалации на нарушение SLA.
///
/// Алгоритм:
/// 1. Запрашиваем все активные эскалации (статусы: New, InProgress, WaitingForResponse) + их SLA
/// 2. Если список пустой => выходим (опять ждем час), если не пустой, то проходим по каждой эскалации
/// 3. У каждой эскалации вычисляем процент израсходованного времени SLA:
/// elapsed = текущее время - время создания эскалации (в минутах) (прошло времени)
/// slaPercent = elapsed / resolutionTimeMinutes * 100
/// 4. Если меньше 80 => берем след. эскалацию,
/// иначе же повышаем приоритет на одну ступень эскалации, если уже blocker, то ничего не меняем
/// 5. Также, если больше 100, то уведомляем юзеров, иначе переходим к следущей
/// 6. Если уведомляем, то запрашиваем юзеров и публикуем событие SlaBreached в RabbitMq
/// 7. Ждем 60 минут
/// </summary>
public sealed class SlaMonitorWorker : BackgroundService
{
    // Минимальный процент для повышения статуса на 1
    private const double WarningThresholdPercent = 80.0;
    // Минимальный процент для уведомления по почту
    private const double BreachThresholdPercent  = 100.0;

    private readonly IEscalationServiceClient _escalationClient;
    private readonly IUserServiceClient _userClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly SchedulerOptions _options;
    private readonly ILogger<SlaMonitorWorker> _logger;

    public SlaMonitorWorker(
        IEscalationServiceClient escalationClient,
        IUserServiceClient userClient,
        IPublishEndpoint publishEndpoint,
        IOptions<SchedulerOptions> options,
        ILogger<SlaMonitorWorker> logger)
    {
        _escalationClient = escalationClient;
        _userClient = userClient;
        _publishEndpoint = publishEndpoint;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SlaMonitorWorker started. Interval: {Interval} min",
            _options.CheckIntervalMinutes);

        // Задержка на всякий случай, если другие сервисы не поднялись (хотя в докере можно в композе настроить и так,
        // но на всякий сделаю
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SLA check failed");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(_options.CheckIntervalMinutes),
                stoppingToken);
        }
    }

    private async Task RunCheckAsync(CancellationToken ct)
    {
        // 1. Получаем активные эскалации
        var items = await _escalationClient.GetSlaCheckItemsAsync(ct);

        if (items.Count == 0)
        {
            _logger.LogInformation("SLA check: no active escalations");
            return;
        }

        _logger.LogInformation("SLA check: processing {Count} escalations", items.Count);

        var now = DateTimeOffset.UtcNow;
        int escalated = 0, breached = 0;

        foreach (var item in items)
        {
            // 2. Считаем % израсходованного SLA-времени
            var elapsedMinutes = (now - item.CreatedAt).TotalMinutes;
            var slaPercent     = elapsedMinutes / item.ResolutionTimeMinutes * 100.0;

            _logger.LogDebug(
                "Escalation {Id} '{Name}': elapsed={Elapsed:F0}min / {Sla}min = {Pct:F1}%",
                item.Id, item.Name, elapsedMinutes, item.ResolutionTimeMinutes, slaPercent);

            if (slaPercent < WarningThresholdPercent)
                continue;

            // 3. Повышаем приоритет при 80% или 100%
            await _escalationClient.EscalatePriorityAsync(item.Id, ct);
            escalated++;

            // 4. При 100% — уведомляем исполнителей
            if (slaPercent >= BreachThresholdPercent)
            {
                await NotifyAssigneesAsync(item, slaPercent, ct);
                breached++;
            }
        }

        _logger.LogInformation(
            "SLA check done: {Escalated} priorities escalated, {Breached} SLA breaches notified",
            escalated, breached);
    }

    private async Task NotifyAssigneesAsync(
        Models.SlaCheckItem item,
        double slaPercent,
        CancellationToken ct)
    {
        if (item.Assignees.Count == 0)
        {
            _logger.LogWarning("Escalation {Id} has no assignees to notify", item.Id);
            return;
        }

        foreach (var assignee in item.Assignees)
        {
            try
            {
                var user = await _userClient.GetUserProfileAsync(assignee.UserId, ct);

                if (user is null)
                    continue;

                await _publishEndpoint.Publish(new SlaBreached(
                    EscalationId: item.Id,
                    EscalationName: item.Name,
                    ProjectName: item.ProjectName,
                    Priority: item.Priority,
                    SlaPercent: Math.Round(slaPercent, 1),
                    AssigneeEmail: user.Email,
                    AssigneeFirstName: user.FirstName,
                    BreachedAt: DateTimeOffset.UtcNow),
                    ct);

                _logger.LogInformation(
                    "SlaBreached published: escalation {Id} → {Email} (SLA {Pct:F1}%)",
                    item.Id, user.Email, slaPercent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error notifying assignee {UserId} for escalation {Id}",
                    assignee.UserId, item.Id);
            }
        }
    }
}
