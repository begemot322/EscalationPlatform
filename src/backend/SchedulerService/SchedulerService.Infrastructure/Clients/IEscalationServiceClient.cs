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
    /// <param name="id">Идентификатор эскалации.</param>
    /// <param name="ct">Токен отмены</param>
    Task EscalatePriorityAsync(long id, CancellationToken ct);
}