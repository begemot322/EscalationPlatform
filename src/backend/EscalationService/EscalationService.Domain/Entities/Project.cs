using EscalationService.Domain.Common;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Корневой агрегат проекта.
/// Группирует эскалации и управляет доступом через владельцев.
/// </summary>
public sealed class Project : BaseEntity
{
    /// <summary>
    /// Название проекта.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Описание проекта.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Уникальный код проекта (например, "PROJ").
    /// Используется для идентификации в URL или ссылках.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Флаг активности проекта.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Дата и время архивации проекта.
    /// </summary>
    public DateTimeOffset? ArchivedAt { get;  set; }

    /// <summary>
    /// Владельцы проекта.
    /// </summary>
    public ICollection<ProjectOwner> Owners { get; set; } = new List<ProjectOwner>();
}