using EscalationService.Domain.Common;
using EscalationService.Domain.Enums;

namespace EscalationService.Domain.Entities;

/// <summary>
/// Комментарий к эскалации.
/// Может быть пользовательским, системным или внутренней заметкой.
/// </summary>
public sealed class Comment : BaseEntity
{
    /// <summary>
    /// Текст комментария.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Флаг, указывающий, является ли комментарий системным.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Тип комментария.
    /// </summary>
    public CommentType Type { get; set; }

    /// <summary>
    /// Идентификатор пользователя, оставившего комментарий.
    /// Ссылается на внешний сервис.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Идентификатор эскалации, к которой относится комментарий.
    /// </summary>
    public long EscalationId { get; set; }
    
    /// <summary>
    /// Навигационное поле.
    /// Эскалация.
    /// </summary>
    public Escalation Escalation { get; set; } = null!;
}