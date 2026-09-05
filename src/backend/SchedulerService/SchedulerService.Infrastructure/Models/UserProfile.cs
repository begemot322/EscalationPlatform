namespace SchedulerService.Models;

/// <summary>
/// Профиль пользователя, полученный из UserService.
/// Используется для получения email при отправке уведомлений о нарушении SLA.
/// </summary>
/// <param name="Id">Уникальный идентификатор пользователя.</param>
/// <param name="FirstName">Имя пользователя.</param>
/// <param name="LastName">Фамилия пользователя.</param>
/// <param name="Email">Email-адрес пользователя.</param>
public sealed record UserProfile(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
