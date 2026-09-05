namespace EscalationPlatform.Shared.Result;

/// <summary>
/// Определяет категорию ошибки
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Ошибка валидации запроса или нарушение бизнес-правил. Соответствует <c>400 Bad Request</c>.
    /// </summary>
    Validation,

    /// <summary>
    /// Запрашиваемый ресурс не найден. Соответствует <c>404 Not Found</c>.
    /// </summary>
    NotFound,

    /// <summary>
    /// Запрос конфликтует с текущим состоянием ресурса (например, дубликаты или нарушение уникальности). Соответствует <c>409 Conflict</c>.
    /// </summary>
    Conflict,

    /// <summary>
    /// Отсутствует или неверна аутентификация (пользователь не вошёл в систему). Соответствует <c>401 Unauthorized</c>.
    /// </summary>
    AccessUnAuthorized,

    /// <summary>
    /// Аутентификация пройдена, но у пользователя нет прав на выполнение операции. Соответствует <c>403 Forbidden</c>.
    /// </summary>
    AccessForbidden,

    /// <summary>
    /// Неожиданная внутренняя ошибка сервера. Соответствует <c>500 Internal Server Error</c>.
    /// </summary>
    Failure
}