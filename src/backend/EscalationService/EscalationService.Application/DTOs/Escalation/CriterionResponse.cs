namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Ответ с данными критерия для выполнения эскалации
/// </summary>
/// <param name="Id">Идентификатор критерия</param>
/// <param name="Name">Название критерия</param>
/// <param name="Description">Описание критерия</param>
/// <param name="IsCompleted">Статус выполнения критерия</param>
public sealed record CriterionResponse(long Id, string Name, string? Description, bool IsCompleted);
