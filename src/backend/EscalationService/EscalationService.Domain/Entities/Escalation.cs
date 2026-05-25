using EscalationService.Domain.Common;
using EscalationService.Domain.Enums;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Сущность эскалации
/// </summary>
public sealed class Escalation : BaseEntity
{
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description  { get; set; }
    
    /// <summary>
    /// Текущий статус эскалации.
    /// </summary>
    public EscalationStatus Status { get; set; }
    
    /// <summary>
    /// Приоритет эскалации.
    /// </summary>
    public EscalationPriority Priority { get; set; }
    
    /// <summary>
    /// Тип эскалации.
    /// </summary>
    public EscalationType Type { get; set; }
    
    /// <summary>
    /// Дата и время первого ответа на эскалацию.
    /// </summary>
    public DateTimeOffset? FirstResponseAt { get; set; }
    
    /// <summary>
    /// Дата и время решения эскалации.
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }
    
    /// <summary>
    /// Дата и время закрытия эскалации.
    /// </summary>
    public DateTimeOffset? ClosedAt { get; set; }
    
    /// <summary>
    /// Идентификатор автора эскалации.
    /// Ссылается на пользователя во внешнем сервисе аутентификации.
    /// </summary>
    public Guid AuthorId { get; set; }
    
    /// <summary>
    /// Идентификатор политики SLA, применяемой к эскалации.
    /// </summary>
    public long SlaPolicyId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Политика SLA.
    /// </summary>
    public SlaPolicy SlaPolicy { get; set; } = null!;
    
    /// <summary>
    /// Идентификатор проекта, к которому относится эскалация.
    /// </summary>
    public long ProjectId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Проект.
    /// </summary>
    public Project Project { get; set; } = null!;
    
    /// <summary>
    /// Навигационное поле.
    /// Метрики эскалации (собираются после закрытия).
    /// </summary>
    public EscalationMetrics? Metrics { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Участники эскалации.
    /// </summary>
    public ICollection<EscalationParticipant> Participants { get; set; } = new List<EscalationParticipant>();
    
    /// <summary>
    /// Навигационное поле.
    /// Коллекция комментариев к эскалации.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    
    /// <summary>
    /// Навигационное поле.
    /// Критерии выполнения.
    /// </summary>
    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
    
    /// <summary>
    /// Навигационное поле.
    /// Вложения.
    /// </summary>
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}