namespace SchedulerService.Models;

/// <summary>
/// Данные эскалации для SLA-мониторинга, полученные из EscalationService.
/// </summary>
/// <param name="Id">Уникальный идентификатор эскалации.</param>
/// <param name="Name">Название эскалации.</param>
/// <param name="ProjectName">Название проекта, к которому относится эскалация.</param>
/// <param name="Priority">Текущий приоритет эскалации.</param>
/// <param name="ResponseTimeMinutes">Максимальное время первого ответа по SLA в минутах.</param>
/// <param name="ResolutionTimeMinutes">Максимальное время разрешения по SLA в минутах.</param>
/// <param name="CreatedAt">Дата и время создания эскалации (UTC).</param>
/// <param name="Assignees">Список исполнителей, назначенных на эскалацию.</param>
public sealed record SlaCheckItem(
    long Id,
    string Name,
    string ProjectName,
    string Priority,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SlaCheckParticipant> Assignees);

/// <summary>
/// Исполнитель эскалации.
/// </summary>
/// <param name="UserId">Идентификатор пользователя в UserService.</param>
public sealed record SlaCheckParticipant(Guid UserId);
