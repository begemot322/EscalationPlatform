namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Элемент критерия при обновлении эскалации.
/// Если список Criteria передан — заменяет текущий набор критериев целиком.
/// </summary>
/// <param name="Name">Название критерия</param>
/// <param name="IsCompleted">Флаг выполнения</param>
public sealed record CriterionUpdateItem(string Name, bool IsCompleted);
