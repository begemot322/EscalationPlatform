namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Запрос на создание нового проекта
/// </summary>
/// <param name="Name">Название проекта</param>
/// <param name="Description">Описание проекта (необязательно)</param>
/// <param name="Owners">Список владельцев проекта с их ролями (необязательно)</param>
public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    IReadOnlyList<AddProjectOwnerRequest>? Owners);
