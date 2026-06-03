namespace EscalationService.Application.DTOs.Escalation;

/// <summary>
/// Статистика конкретного пользователя как исполнителя эскалаций.
/// Используется менеджерами и администраторами для оценки эффективности команды.
/// </summary>
public sealed record UserStatsItem(
    Guid UserId,

    // ── Общее ──────────────────────────────────────────────────────────────
    int Total,
    int Active,
    int Closed,
    int Cancelled,

    // ── Из EscalationMetrics (только закрытые) ─────────────────────────────
    int ClosedWithMetrics,
    double? AvgFirstResponseMinutes,
    double? AvgResolutionMinutes,
    int SlaViolated,
    int SlaCompliant,

    // ── Расчётные показатели ───────────────────────────────────────────────
    int CompletionPercent,
    int EfficiencyScore
);
