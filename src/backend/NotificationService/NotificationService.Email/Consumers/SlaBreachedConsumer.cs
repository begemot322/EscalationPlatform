using EscalationPlatform.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace NotificationService.Email.Consumers;

public sealed class SlaBreachedConsumer : IConsumer<SlaBreached>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SlaBreachedConsumer> _logger;
    private static readonly string HtmlTemplate;

    static SlaBreachedConsumer()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "SlaBreachedEmail.html");
        HtmlTemplate = File.ReadAllText(path);
    }

    public SlaBreachedConsumer(IEmailSender emailSender, ILogger<SlaBreachedConsumer> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SlaBreached> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Sending SLA breach notification to {Email} for escalation {Id} (SLA {Pct:F1}%)",
            msg.AssigneeEmail, msg.EscalationId, msg.SlaPercent);

        var subject = $"[WaveAccess] Нарушение SLA: {msg.EscalationName}";
        var html = BuildHtml(msg);

        await _emailSender.SendAsync(msg.AssigneeEmail, subject, html);
    }

    private static string BuildHtml(SlaBreached msg)
        => HtmlTemplate
            .Replace("{{FirstName}}", msg.AssigneeFirstName)
            .Replace("{{EscalationName}}", msg.EscalationName)
            .Replace("{{ProjectName}}", msg.ProjectName)
            .Replace("{{Priority}}", msg.Priority)
            .Replace("{{SlaPercent}}", msg.SlaPercent.ToString("F1"))
            .Replace("{{BreachedAt}}", msg.BreachedAt.ToString("dd.MM.yyyy HH:mm"));
}
