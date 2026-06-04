namespace EscalationPlatform.Shared.Messages;

/// <summary>
/// Событие назначения участника на эскалацию.
/// Публикуется EscalationService при добавлении нового исполнителя или наблюдателя.
/// </summary>
/// <param name="EscalationId">Идентификатор эскалации</param>
/// <param name="EscalationName">Название эскалации</param>
/// <param name="ProjectName">Название проекта</param>
/// <param name="RecipientEmail">Email назначенного участника</param>
/// <param name="RecipientFirstName">Имя назначенного участника</param>
/// <param name="Role">Роль: «Исполнитель» или «Наблюдатель»</param>
/// <param name="AssignedAt">Момент назначения</param>
public record ParticipantAssigned(
    long EscalationId,
    string EscalationName,
    string ProjectName,
    string RecipientEmail,
    string RecipientFirstName,
    string Role,
    DateTimeOffset AssignedAt);
