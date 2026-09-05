using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Ответ с данными комментария к эскалации
/// </summary>
/// <param name="Id">Идентификатор комментария</param>
/// <param name="Text">Текст комментария</param>
/// <param name="IsSystem">Флаг системного комментария</param>
/// <param name="Type">Тип комментария</param>
/// <param name="UserId">Идентификатор автора</param>
/// <param name="CreatedAt">Дата создания</param>
public sealed record CommentResponse(
    long Id,
    string Text,
    bool IsSystem,
    CommentType Type,
    Guid UserId,
    DateTimeOffset CreatedAt);
