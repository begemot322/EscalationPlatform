using SchedulerService.Models;

namespace SchedulerService.Clients;

/// <summary>
/// Клиент для взаимодействия с внутренними эндпоинтами EscalationService.
/// </summary>
public interface IEscalationServiceClient
{
    /// <summary>
    /// Возвращает список активных эскалаций с данными SLA для мониторинга.
    /// </summary>
    Task<IReadOnlyList<SlaCheckItem>> GetSlaCheckItemsAsync(CancellationToken ct);

    /// <summary>
    /// Повышает приоритет эскалации на одну ступень.
    /// </summary>
    Task EscalatePriorityAsync(long id, CancellationToken ct);

    /// <summary>
    /// Возвращает месячную статистику по всем проектам за указанный период.
    /// </summary>
    Task<IReadOnlyList<ProjectMonthlyStats>> GetMonthlyStatsAsync(int year, int month, CancellationToken ct);
}