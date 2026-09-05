using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.DTOs.SlaPolicy;

namespace EscalationService.Application.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса управления SLA политиками
/// </summary>
public interface ISlaPolicyService
{
    /// <summary>
    /// Получает список всех SLA политик с пагинацией
    /// </summary>
    /// <param name="pageParams">Параметры пагинации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<PagedResult<SlaPolicyResponse>>> GetAllAsync(PageParams? pageParams, CancellationToken ct);
    
    /// <summary>
    /// Получает SLA политику по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор SLA политики</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<SlaPolicyResponse>> GetByIdAsync(long id, CancellationToken ct);
    
    /// <summary>
    /// Создаёт новую SLA политику
    /// </summary>
    /// <param name="request">Запрос на создание SLA политики</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> CreateAsync(CreateSlaPolicyRequest request, CancellationToken ct);
    
    /// <summary>
    /// Обновляет существующую SLA политику
    /// </summary>
    /// <param name="id">Идентификатор SLA политики</param>
    /// <param name="request">Запрос на обновление SLA политики</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> UpdateAsync(long id, UpdateSlaPolicyRequest request, CancellationToken ct);
    
    /// <summary>
    /// Удаляет SLA политику по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор SLA политики</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> DeleteAsync(long id, CancellationToken ct);
}