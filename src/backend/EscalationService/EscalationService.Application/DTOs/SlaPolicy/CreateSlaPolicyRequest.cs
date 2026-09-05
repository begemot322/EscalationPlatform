using EscalationService.Domain.Enums;

namespace EscalationService.Application.DTOs.SlaPolicy;

/// <summary>
/// Запрос на создание политики (SLA)
/// </summary>
/// <param name="Name">Название SLA политики</param>
/// <param name="Description">Описание политики (необязательно)</param>
/// <param name="MinPriorityLevel">Минимальный уровень приоритета для применения политики</param>
/// <param name="ResponseTimeMinutes">Время реакции на инцидент (в минутах)</param>
/// <param name="ResolutionTimeMinutes">Время решения проблемы (в минутах)</param>
public sealed record CreateSlaPolicyRequest(
    string Name,
    string? Description,
    EscalationPriority MinPriorityLevel,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes);
