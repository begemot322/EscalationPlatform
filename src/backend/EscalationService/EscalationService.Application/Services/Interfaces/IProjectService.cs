using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.DTOs.Project;

namespace EscalationService.Application.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса управления проектами
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Получает список всех проектов с фильтрацией и пагинацией
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <param name="pageParams">Параметры пагинации</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<PagedResult<ProjectResponse>>> GetAllAsync(ProjectFilter? filter, PageParams? pageParams, CancellationToken ct);
    
    /// <summary>
    /// Получает проект по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<ProjectResponse>> GetByIdAsync(long id, CancellationToken ct);
    
    /// <summary>
    /// Создаёт новый проект
    /// </summary>
    /// <param name="request">Запрос на создание проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> CreateAsync(CreateProjectRequest request, CancellationToken ct);
    
    /// <summary>
    /// Обновляет существующий проект
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="request">Запрос на обновление проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> UpdateAsync(long id, UpdateProjectRequest request, CancellationToken ct);
    
    /// <summary>
    /// Архивирует проект
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> ArchiveAsync(long id, CancellationToken ct);
    
    /// <summary>
    /// Удаляет проект по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result> DeleteAsync(long id, CancellationToken ct);

    /// <summary>
    /// Импортирует проекты из Excel файла (.xlsx).
    /// Проекты с кодами, уже существующими в БД, пропускаются.
    /// </summary>
    /// <param name="fileStream">Поток .xlsx файла</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Result<ProjectImportResult>> ImportFromExcelAsync(Stream fileStream, CancellationToken ct);
}