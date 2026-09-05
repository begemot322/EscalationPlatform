using EscalationPlatform.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace NotificationService.Email.Consumers;

/// <summary>
/// Получает событие назначения участника на эскалацию и отправляет email-уведомление.
/// </summary>
public sealed class ParticipantAssignedConsumer : IConsumer<ParticipantAssigned>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ParticipantAssignedConsumer> _logger;
    private static readonly string HtmlTemplate;

    static ParticipantAssignedConsumer()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "ParticipantAssignedEmail.html");
        HtmlTemplate = File.ReadAllText(path);
    }

    public ParticipantAssignedConsumer(IEmailSender emailSender, ILogger<ParticipantAssignedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ParticipantAssigned> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Sending participant assigned notification to {Email} for escalation {Id} (role: {Role})",
            msg.RecipientEmail, msg.EscalationId, msg.Role);

        var subject = $"[WaveAccess] Вы назначены на эскалацию: {msg.EscalationName}";
        var html = BuildHtml(msg);

        await _emailSender.SendAsync(msg.RecipientEmail, subject, html);
    }

    private static string BuildHtml(ParticipantAssigned msg)
        => HtmlTemplate
            .Replace("{{FirstName}}", msg.RecipientFirstName)
            .Replace("{{EscalationName}}", msg.EscalationName)
            .Replace("{{ProjectName}}", msg.ProjectName)
            .Replace("{{Role}}", msg.Role)
            .Replace("{{AssignedAt}}", msg.AssignedAt.ToString("dd.MM.yyyy HH:mm"));
}
