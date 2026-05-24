using EscalationPlatform.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace NotificationService.Email.Consumers;

public class OtpCodeRequestedConsumer : IConsumer<OtpCodeRequested>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OtpCodeRequestedConsumer> _logger;
    private static readonly string HtmlTemplate;

    static OtpCodeRequestedConsumer()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "OtpEmail.html");
        HtmlTemplate = File.ReadAllText(path);
    }
    
    public OtpCodeRequestedConsumer(IEmailSender emailSender, ILogger<OtpCodeRequestedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<OtpCodeRequested> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Отправка OTP-кода на {Email}", msg.Email);

        var subject = "Ваш код подтверждения";
        var html = BuildHtml(msg.FirstName, msg.Code, msg.ExpiresMinutes);

        await _emailSender.SendAsync(msg.Email, subject, html);
    }
    
    private static string BuildHtml(string firstName, string code, int expiresMinutes)
        => HtmlTemplate
            .Replace("{{FirstName}}", firstName)
            .Replace("{{Code}}", code)
            .Replace("{{ExpiresMinutes}}", expiresMinutes.ToString());
}