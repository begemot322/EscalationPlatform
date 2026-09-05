namespace EscalationPlatform.Shared.Messages;

/// <summary>
/// Запрос на отправку OTP-кода пользователю на email.
/// </summary>
/// <param name="Email">Email получателя.</param>
/// <param name="FirstName">Имя пользователя.</param>
/// <param name="Code">OTP-код.</param>
/// <param name="ExpiresMinutes">Время жизни кода в минутах.</param>
public record OtpCodeRequested(
    string Email,
    string FirstName,
    string Code,
    int ExpiresMinutes);