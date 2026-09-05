namespace UserService.Application.DTOs;

/// <summary>
/// Ответ после успешной аутентификации.
/// </summary>
/// <param name="Token">JWT токен доступа.</param>
public record AuthResponse(string Token);
