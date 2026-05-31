using EscalationPlatform.Shared.Messages;
using MassTransit;
using SchedulerService.Clients;

namespace SchedulerService.Services;

/// <summary>
/// Фоновый Worker - 1-го числа каждого месяца собирает статистику по проектам
/// и публикует MonthlyReportRequested для каждого владельца проекта.
///
/// Алгоритм:
/// 1. Считает задержку до 1-го числа следующего месяца 09:00 UTC и засыпает
/// 2. Запрашивает статистику за прошедший месяц у EscalationService
/// 3. Для каждого проекта - берет владельцев (Role == Owner) и получает их email из UserService
/// 4. Публикует MonthlyReportRequested в RabbitMQ на каждого владельца
/// 5. Цикл повторяется
/// </summary>
public sealed class MonthlyReportWorker : BackgroundService
{
    private readonly IEscalationServiceClient _escalationClient;
    private readonly IUserServiceClient _userClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MonthlyReportWorker> _logger;

    public MonthlyReportWorker(
        IEscalationServiceClient escalationClient,
        IUserServiceClient userClient,
        IPublishEndpoint publishEndpoint,
        ILogger<MonthlyReportWorker> logger)
    {
        _escalationClient = escalationClient;
        _userClient = userClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonthlyReportWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextRun = ComputeNextRun(now);
            var delay = nextRun - now;

            _logger.LogInformation(
                "MonthlyReportWorker: next run in {Hours:F1}h at {NextRun:u}",
                delay.TotalHours, nextRun);

            await Task.Delay(delay, stoppingToken);

            // После пробуждения — запускаем отчет за прошедший месяц
            now = DateTimeOffset.UtcNow;
            var targetYear = now.Month == 1 ? now.Year - 1 : now.Year;
            var targetMonth = now.Month == 1 ? 12 : now.Month - 1;

            try
            {
                await RunReportAsync(targetYear, targetMonth, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "MonthlyReportWorker: failed for {Year}-{Month:D2}", targetYear, targetMonth);
            }
        }
    }

    private async Task RunReportAsync(int year, int month, CancellationToken ct)
    {
        _logger.LogInformation("Monthly report: collecting stats for {Year}-{Month:D2}", year, month);

        var stats = await _escalationClient.GetMonthlyStatsAsync(year, month, ct);

        if (stats.Count == 0)
        {
            _logger.LogInformation("Monthly report: no projects with escalations for {Year}-{Month:D2}", year, month);
            return;
        }

        var published = 0;

        foreach (var project in stats)
        {
            if (project.TotalCreated == 0)
                continue;

            if (project.OwnerUserIds.Count == 0)
            {
                _logger.LogWarning(
                    "Monthly report: project '{Project}' has no owners, skipping",
                    project.ProjectName);
                continue;
            }

            foreach (var ownerId in project.OwnerUserIds)
            {
                try
                {
                    var user = await _userClient.GetUserProfileAsync(ownerId, ct);

                    if (user is null)
                    {
                        _logger.LogWarning(
                            "Monthly report: owner {UserId} not found in UserService", ownerId);
                        continue;
                    }

                    await _publishEndpoint.Publish(new MonthlyReportRequested(
                        ProjectName: project.ProjectName,
                        Year: year,
                        Month: month,
                        TotalCreated: project.TotalCreated,
                        ByStatus: project.ByStatus,
                        ByPriority: project.ByPriority,
                        SlaBreachCount: project.SlaBreachCount,
                        AverageResolutionHours: project.AverageResolutionHours,
                        OwnerEmail: user.Email,
                        OwnerFirstName: user.FirstName),
                        ct);

                    _logger.LogInformation(
                        "MonthlyReportRequested published: project '{Project}' → {Email}",
                        project.ProjectName, user.Email);

                    published++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Monthly report: error processing owner {UserId} for project '{Project}'",
                        ownerId, project.ProjectName);
                }
            }
        }

        _logger.LogInformation(
            "Monthly report done: {Published} messages published for {Year}-{Month:D2}",
            published, year, month);
    }

    /// <summary>
    /// Следующий запуск - 1-е число следующего месяца в 09:00 UTC.
    /// </summary>
    private static DateTimeOffset ComputeNextRun(DateTimeOffset now)
        => new DateTimeOffset(now.Year, now.Month, 1, 9, 0, 0, TimeSpan.Zero).AddMonths(1);
}
