namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Месячная статистика по одному проекту.
/// Возвращается внутренним эндпоинтом для SchedulerService.
/// </summary>
public sealed record ProjectMonthlyStats(
    long ProjectId,
    string ProjectName,
    int Year,
    int Month,
    int TotalCreated,
    Dictionary<string, int> ByStatus,
    Dictionary<string, int> ByPriority,
    int SlaBreachCount,
    double AverageResolutionHours,
    IReadOnlyList<Guid> OwnerUserIds);
