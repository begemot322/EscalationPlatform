namespace EscalationPlatform.Shared.Messages;

/// <summary>
/// Запрос на генерацию и отправку ежемесячного отчета по проекту.
/// Публикуется SchedulerService 1-го числа каждого месяца.
/// </summary>
/// <param name="ProjectName">Название проекта.</param>
/// <param name="Year">Год отчетного периода.</param>
/// <param name="Month">Месяц отчетного периода (1–12).</param>
/// <param name="TotalCreated">Всего создано эскалаций за месяц.</param>
/// <param name="ByStatus">Количество эскалаций по каждому статусу.</param>
/// <param name="ByPriority">Количество эскалаций по каждому приоритету.</param>
/// <param name="SlaBreachCount">Число нарушений SLA за период.</param>
/// <param name="AverageResolutionHours">Среднее время решения в часах (только закрытые).</param>
/// <param name="OwnerEmail">Email владельца проекта - получателя отчета.</param>
/// <param name="OwnerFirstName">Имя владельца для обращения в письме.</param>
public record MonthlyReportRequested(
    string ProjectName,
    int Year,
    int Month,
    int TotalCreated,
    Dictionary<string, int> ByStatus,
    Dictionary<string, int> ByPriority,
    int SlaBreachCount,
    double AverageResolutionHours,
    string OwnerEmail,
    string OwnerFirstName);
