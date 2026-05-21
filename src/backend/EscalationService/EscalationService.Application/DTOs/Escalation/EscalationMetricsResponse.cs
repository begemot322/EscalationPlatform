namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Ответ с метриками эскалации
/// </summary>
/// <param name="FirstResponseTimeMinutes">Время первого ответа (в минутах)</param>
/// <param name="ResolutionTimeMinutes">Время решения проблемы (в минутах)</param>
/// <param name="TimeToCloseMinutes">Время до закрытия эскалации (в минутах)</param>
/// <param name="SlaViolated">Флаг нарушения SLA</param>
/// <param name="TotalComments">Общее количество комментариев</param>
/// <param name="TotalParticipants">Общее количество участников</param>
public sealed record EscalationMetricsResponse(
    int FirstResponseTimeMinutes,
    int ResolutionTimeMinutes,
    int TimeToCloseMinutes,
    bool SlaViolated,
    int TotalComments,
    int TotalParticipants);
