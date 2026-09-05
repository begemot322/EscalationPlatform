using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// DTO для добавления владельца проекта
/// </summary>
/// <param name="UserId">ID пользователя</param>
/// <param name="Role">Роль в проекте</param>
public sealed record AddProjectOwnerRequest(Guid UserId, ProjectRole Role);
