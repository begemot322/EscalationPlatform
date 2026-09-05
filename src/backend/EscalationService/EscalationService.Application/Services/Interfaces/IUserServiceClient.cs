namespace EscalationService.Application.Services.Interfaces;

/// <summary>
/// Клиент для получения данных пользователей из UserService.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Возвращает пользователей по списку email-адресов.
    /// </summary>
    Task<List<UserLookupResult>> GetByEmailsAsync(IEnumerable<string> emails, CancellationToken ct);

    /// <summary>
    /// Возвращает пользователей по списку идентификаторов.
    /// </summary>
    Task<List<UserLookupResult>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
}

/// <summary>
/// Краткая информация о пользователе, полученная из UserService
/// </summary>
public sealed record UserLookupResult(Guid Id, string Email, string FirstName);
