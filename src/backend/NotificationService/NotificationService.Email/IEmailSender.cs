namespace NotificationService.Email;

/// <summary>
/// Интерфейс для отправки Email сообщения
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Отправляет email сообщение.
    /// </summary>
    /// <param name="to">Email получателя</param>
    /// <param name="subject">Тема письма</param>
    /// <param name="htmlBody">HTML содержимое письма.</param>
    Task SendAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// Отправляет email с файловым вложением.
    /// </summary>
    /// <param name="to">Email получателя</param>
    /// <param name="subject">Тема письма</param>
    /// <param name="htmlBody">HTML содержимое письма.</param>
    /// <param name="attachment">Байты вложения.</param>
    /// <param name="attachmentFileName">Имя файла вложения (с расширением).</param>
    Task SendWithAttachmentAsync(
        string to,
        string subject,
        string htmlBody,
        byte[] attachment,
        string attachmentFileName);
}