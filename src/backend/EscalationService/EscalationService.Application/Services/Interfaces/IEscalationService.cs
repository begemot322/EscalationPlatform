using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.DTOs.Escalation;
using EscalationService.Application.DTOs.Project;

namespace EscalationService.Application.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса управления эскалациями
/// </summary>
public interface IEscalationService
{
    /// <summary>
    /// Получает список всех эскалаций с фильтрацией, сортировкой и пагинацией
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <param name="sortParams">Параметры сортировки</param>
    /// <param name="pageParams">Параметры пагинации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<PagedResult<EscalationSummaryResponse>>> GetAllAsync(
        EscalationFilter? filter, SortParams? sortParams, PageParams? pageParams, CancellationToken ct);
    
    /// <summary>
    /// Получает полную информацию об эскалации по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор эскалации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<EscalationResponse>> GetByIdAsync(long id, CancellationToken ct);
    
    /// <summary>
    /// Создаёт новую эскалацию
    /// </summary>
    /// <param name="request">Запрос на создание эскалации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> CreateAsync(CreateEscalationRequest request, CancellationToken ct);
    
    /// <summary>
    /// Обновляет существующую эскалацию
    /// </summary>
    /// <param name="id">Идентификатор эскалации</param>
    /// <param name="request">Запрос на обновление эскалации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> UpdateAsync(long id, UpdateEscalationRequest request, CancellationToken ct);
    
    /// <summary>
    /// Удаляет эскалацию по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор эскалации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> DeleteAsync(long id, CancellationToken ct);

    /// <summary>
    /// Добавляет пользовательский комментарий к эскалации.
    /// </summary>
    Task<Result> AddCommentAsync(long escalationId, CreateCommentRequest request, CancellationToken ct);

    /// <summary>
    /// ВНУТРЕННИЙ ЭНДПОИНТ.
    /// Возвращает активные эскалации с данными SLA для мониторинга.
    /// </summary>
    Task<Result<IReadOnlyList<SlaCheckItem>>> GetSlaCheckItemsAsync(CancellationToken ct);

    /// <summary>
    /// ВНУТРЕННИЙ ЭНДПОИНТ.
    /// Повышает приоритет эскалации на одну ступень.
    /// </summary>
    Task<Result> EscalatePriorityAsync(long id, CancellationToken ct);

    /// <summary>
    /// ВНУТРЕННИЙ ЭНДПОИНТ.
    /// Возвращает статистику по всем проектам за указанный месяц.
    /// Используется SchedulerService для формирования ежемесячных отчетов.
    /// </summary>
    Task<Result<IReadOnlyList<ProjectMonthlyStats>>> GetMonthlyStatsAsync(int year, int month, CancellationToken ct);

    /// <summary>
    /// Возвращает статистику по всем исполнителям.
    /// Используется менеджерами и администраторами.
    /// </summary>
    Task<Result<IReadOnlyList<UserStatsItem>>> GetUsersStatsAsync(CancellationToken ct);

}
