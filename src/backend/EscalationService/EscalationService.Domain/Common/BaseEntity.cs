namespace EscalationService.Domain.Common;

/// <summary>
/// Базовый класс для всех доменных сущностей
/// </summary>
public abstract class BaseEntity : ICreatableAuditedEntity, IUpdatableAuditedEntity
{
    /// <summary>
    /// Уникальный идентификатор сущности.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Дата и время создания сущности.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления сущности.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}