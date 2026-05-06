namespace EscalationService.Domain.Enums;

/// <summary>
/// Тип комментария к эскалации.
/// </summary>
public enum CommentType
{
    /// <summary>Пользовательский комментарий, оставленный человеком.</summary>
    User = 1,

    /// <summary>Системный комментарий, сгенерированный автоматически.</summary>
    System = 2,

    /// <summary>Внутренняя заметка, видимая только сотрудникам.</summary>
    Note = 3
}