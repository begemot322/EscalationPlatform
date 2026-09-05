namespace UserService.Api;

/// <summary>
/// Методы расширения для работы с куками аутентификации.
/// </summary>
public static class CookieExtensions
{
    private const string AccessTokenCookieName = "access_token";

    /// <summary>
    /// Устанавливает access token в куки.
    /// </summary>
    public static void SetAccessToken(this HttpResponse response, string token)
        => SetJwtCookie(response, AccessTokenCookieName, token);

    /// <summary>
    /// Удаляет access token из куки.
    /// </summary>
    public static void RemoveAccessToken(this HttpResponse response)
        => RemoveJwtCookie(response, AccessTokenCookieName);

    private static void SetJwtCookie(HttpResponse response, string cookie, string token)
    {
        response.Cookies.Append(cookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true
        });
    }

    private static void RemoveJwtCookie(HttpResponse response, string cookie)
    {
        response.Cookies.Delete(cookie, new CookieOptions
        {
            Path = "/",
            Secure = false,
            SameSite = SameSiteMode.Lax
        });
    }
}