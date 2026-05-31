namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Запрос на добавление комментария к эскалации
/// </summary>
/// <param name="Text">Текст комментария</param>
/// <param name="AuthorId">Идентификатор автора</param>
public sealed record CreateCommentRequest(string Text, Guid AuthorId);
