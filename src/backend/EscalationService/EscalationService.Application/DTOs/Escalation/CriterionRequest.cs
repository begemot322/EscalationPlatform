namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Запрос на создание критерия выполнения эскалации
/// </summary>
/// <param name="Name">Название критерия</param>
public sealed record CriterionRequest(string Name);
