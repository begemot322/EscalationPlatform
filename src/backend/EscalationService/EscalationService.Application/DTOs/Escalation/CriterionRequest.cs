namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Запрос на создание критерия выполнения эскалации
/// </summary>
/// <param name="Name">Название критерия</param>
/// <param name="Description">Описание критерия (необязательно)</param>
public sealed record CriterionRequest(string Name, string? Description);
