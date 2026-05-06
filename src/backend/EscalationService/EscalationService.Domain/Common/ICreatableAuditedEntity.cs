namespace EscalationService.Domain.Common;

/// <summary>
/// Интерфейс для аудита создания сущности
/// </summary>
public interface ICreatableAuditedEntity
{
    /// <summary>
    /// Дата создания сущности
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}