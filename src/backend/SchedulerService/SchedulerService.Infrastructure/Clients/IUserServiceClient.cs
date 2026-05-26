using SchedulerService.Models;

namespace SchedulerService.Clients;

/// <summary>
/// Клиент для взаимодействия с UserService.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Возвращает профиль пользователя по идентификатору.
    /// Возвращает <c>null</c>, если пользователь не найден.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    Task<UserProfile?> GetUserProfileAsync(Guid userId, CancellationToken ct);
}