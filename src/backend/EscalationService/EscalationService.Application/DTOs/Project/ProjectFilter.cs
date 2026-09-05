namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Фильтр для запроса списка проектов
/// </summary>
/// <param name="IsActive">Фильтр по активности проекта (активные/архивные)</param>
/// <param name="Search">Поисковая строка для фильтрации по названию или описанию проекта</param>
public sealed record ProjectFilter(
    bool? IsActive,
    string? Search);
