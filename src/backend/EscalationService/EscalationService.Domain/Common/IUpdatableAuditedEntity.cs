namespace EscalationService.Domain.Common;

/// <summary>
/// Интерфейс для аудита обновления сущности
/// </summary>
public interface IUpdatableAuditedEntity
{
    /// <summary>
    /// Дата обновления сущности
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}