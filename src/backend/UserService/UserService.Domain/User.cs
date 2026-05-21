
namespace UserService.Domain;

/// <summary>
/// Представляет пользователя системы.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Фамилия пользователя.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Email пользователя.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Номер телефона пользователя.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Хэш пароля пользователя.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Имя файла изображения профиля пользователя.
    /// </summary>
    public string? ProfileImageFileName { get; set; }

    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Признак подтвержденного email.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Дата и время создания пользователя.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public ICollection<OtpCode> OtpCodes { get; set; } = [];
}