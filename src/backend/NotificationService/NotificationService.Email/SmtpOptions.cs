using JetBrains.Annotations;

namespace NotificationService.Email;

/// <summary>
/// Настройки для отправки email через SMTP
/// </summary>
[PublicAPI]
public class SmtpOptions
{
    /// <summary>
    /// Имя секции конфигурации SMTP
    /// </summary>
    public const string ConfigurationSectionName = "Smtp";
    
    /// <summary>
    /// SMTP сервер (smtp.gmail.com, smtp.yandex.ru и т.д.)
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Порт SMTP сервера (стандартный 587)
    /// Стандартные значения:
    /// - 587 - рекомендуется, безопасный порт по умолчанию для отправки электронной почты
    /// - 465 - устаревший порт SMTPS (использовать только при необходимости)
    /// - 25 - стандартный порт SMTP  (блокируется из-за проблем со спамом)
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Идентификатор для аутентификации на SMTP сервере (логин)
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Пароль для аутентификации на SMTP сервере
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Email отправителя
    /// </summary>
    public string FromEmail { get; set; } = null!;

    /// <summary>
    /// Имя отправителя, как его увидит получатель
    /// </summary>
    public string FromName { get; set; } = null!;
    
    /// <summary>
    /// Режим шифрования SMTP соединения.
    /// </summary>
    public SslMode EncryptionMode { get; set; } = SslMode.StartTls;
    
    /// <summary>
    /// Массив хостов, которые должен принимать клиент
    /// </summary>
    public string[] AllowedHosts { get; set; } = [];

    /// <summary>
    /// Режим шифрования при подключении к SMTP серверу
    /// </summary>
    public enum SslMode
    {
        /// <summary>
        /// Без шифрования
        /// </summary>
        None,
        
        /// <summary>
        /// SSL/TLS с момента подключения
        /// Обычно используется с портом 465
        /// </summary>
        Ssl,
        
        /// <summary>
        /// STARTTLS (явное включение TLS)
        /// Сначала нешифрованное, затем переключается в защищенное
        /// Обычно используется с портом 587
        /// </summary>
        StartTls
    }
}