namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Ответ с данными проекта
/// </summary>
/// <param name="Id">Идентификатор проекта</param>
/// <param name="Name">Название проекта</param>
/// <param name="Description">Описание проекта</param>
/// <param name="Code">Уникальный код проекта</param>
/// <param name="IsActive">Статус активности проекта (активен/архивирован)</param>
/// <param name="ArchivedAt">Дата архивации проекта</param>
/// <param name="CreatedAt">Дата создания проекта</param>
/// <param name="UpdatedAt">Дата последнего обновления</param>
public sealed record ProjectResponse(
    long Id,
    string Name,
    string? Description,
    string Code,
    bool IsActive,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
