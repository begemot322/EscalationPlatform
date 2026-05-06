using EscalationService.Domain.Enums;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Владелец или участник проекта.
/// Связывает пользователя с проектом и определяет его роль. (many to many)
/// </summary>
public sealed class ProjectOwner
{
    /// <summary>
    /// Идентификатор проекта.
    /// </summary>
    public long ProjectId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Проект.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Роль пользователя в проекте.
    /// </summary>
    public ProjectRole Role { get; set; }

    /// <summary>
    /// Дата и время назначения роли.
    /// </summary>
    public DateTimeOffset AssignedAt { get; set; }
}