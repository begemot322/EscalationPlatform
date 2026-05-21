using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Ответ с данными владельца проекта
/// </summary>
/// <param name="UserId">Идентификатор пользователя-владельца</param>
/// <param name="Role">Роль пользователя в проекте</param>
/// <param name="AssignedAt">Дата и время назначения владельцем</param>
public sealed record ProjectOwnerResponse(
    long UserId,
    ProjectRole Role,
    DateTimeOffset AssignedAt);
