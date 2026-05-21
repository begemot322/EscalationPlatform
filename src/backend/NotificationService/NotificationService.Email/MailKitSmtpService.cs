using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace NotificationService.Email;

public class MailKitSmtpService : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<MailKitSmtpService> _logger;

    public MailKitSmtpService(IOptions<SmtpOptions> options,  ILogger<MailKitSmtpService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        
        client.ServerCertificateValidationCallback = ValidateServerCertificate!;
        
        try
        {
            var security = MapSecurity(_options.EncryptionMode);
            
            await client.ConnectAsync(_options.Host, _options.Port, security);
            await client.AuthenticateAsync(_options.UserName, _options.Password);
            await client.SendAsync(message);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning(ex, "Ошибка аутентификации SMTP: проверьте логин и пароль для {Host}", _options.Host);
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogWarning(ex, "Сервер SMTP {Host} отклонил команду: {Message}", _options.Host, ex.Message);
        }
        catch (SmtpProtocolException ex)
        {
            _logger.LogWarning(ex, "Ошибка протокола SMTP при подключении к {Host}", _options.Host);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка отправки через {Host}", _options.Host);
            throw new InvalidOperationException(
                $"Ошибка отправки через {_options.Host}: {ex.Message}", ex);
        }
        finally {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }

    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // Если ошибок нет, то все норм
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
        
        var host = (string) sender;
        
        // Сертификат недоступен
        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
        {
            _logger.LogError("SSL сертификат недоступен для {Host}", host);
            return false;
        }

        // Несовпадение имени сертификата
        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
        {
            var certHost = certificate is X509Certificate2 certificate2
                ? certificate2.GetNameInfo(X509NameType.SimpleName, false)
                : certificate.Subject;
            
            _logger.LogDebug("Несовпадение имени сертификата: ожидалось {ExpectedHost}, получено {CertHost}", host, certHost);
            
            // Проверяем, есть ли хост из сертификата в allowedHosts
            if (_options.AllowedHosts.Any(allowed => 
                    string.Equals(allowed, certHost, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogDebug("Хост {CertHost} найден в списке разрешенных. Соединение разрешено.", certHost);
                return true;
            }

            _logger.LogError("Хост {CertHost} не найден в списке разрешенных хостов", certHost);
            return false;
        }
        
        // Ошибки цепочки сертификатов
        _logger.LogWarning("Ошибки валидации цепочки SSL сертификата для {Host}:", host);

        foreach (var element in chain.ChainElements)
        {
            if (element.ChainElementStatus.Length == 0)
                continue;

            foreach (var error in element.ChainElementStatus)
            {
                _logger.LogWarning("{Subject}: {Error}", element.Certificate.Subject, error.StatusInformation);
            }
        }
        return false;
    }
    
    private static SecureSocketOptions MapSecurity(SmtpOptions.SslMode mode) => mode switch
    {
        SmtpOptions.SslMode.None => SecureSocketOptions.None,
        SmtpOptions.SslMode.Ssl => SecureSocketOptions.SslOnConnect,
        SmtpOptions.SslMode.StartTls => SecureSocketOptions.StartTls,
        _ => SecureSocketOptions.Auto
    };
}