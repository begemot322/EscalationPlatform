namespace EscalationPlatform.Shared.Messages;

/// <summary>
/// Событие нарушения SLA по эскалации.
/// </summary>
/// <param name="EscalationId">Идентификатор эскалации.</param>
/// <param name="EscalationName">Название эскалации.</param>
/// <param name="ProjectName">Название проекта.</param>
/// <param name="Priority">Текущий приоритет (после повышения).</param>
/// <param name="SlaPercent">Процент израсходованного SLA времени.</param>
/// <param name="AssigneeEmail">Email ответственного исполнителя.</param>
/// <param name="AssigneeFirstName">Имя ответственного исполнителя.</param>
/// <param name="BreachedAt">Момент нарушения SLA.</param>
public record SlaBreached(
    long EscalationId,
    string EscalationName,
    string ProjectName,
    string Priority,
    double SlaPercent,
    string AssigneeEmail,
    string AssigneeFirstName,
    DateTimeOffset BreachedAt);
