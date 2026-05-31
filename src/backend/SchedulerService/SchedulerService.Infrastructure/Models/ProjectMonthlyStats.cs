namespace SchedulerService.Models;

/// <summary>
/// Месячная статистика по одному проекту.
/// Десериализуется из ответа EscalationService.
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
