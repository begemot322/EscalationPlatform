namespace EscalationPlatform.Bus.RabbitMq;

/// <summary>
/// Названия Exchange для RabbitMQ
/// </summary>
public static class Exchanges
{
    /// <summary>
    /// Запрос на отправку OTP-кода на email пользователя.
    /// </summary>
    public const string UserEmailOtpRequested = "user.email_otp.requested";

    /// <summary>
    /// Событие нарушения SLA по эскалации.
    /// </summary>
    public const string EscalationSlaBreached = "escalation.sla.breached";

    /// <summary>
    /// Запрос на генерацию ежемесячного отчета по проекту.
    /// </summary>
    public const string EscalationMonthlyReport = "escalation.monthly.report";
}