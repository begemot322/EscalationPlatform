using EscalationPlatform.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Pdf;

namespace NotificationService.Email.Consumers;

/// <summary>
/// Получает запрос на ежемесячный отчет, генерирует PDF и отправляет его владельцу проекта.
/// </summary>
public sealed class MonthlyReportConsumer : IConsumer<MonthlyReportRequested>
{
    private readonly IEmailSender _emailSender;
    private readonly IPdfReportService _pdfService;
    private readonly ILogger<MonthlyReportConsumer> _logger;
    private static readonly string HtmlTemplate;

    static MonthlyReportConsumer()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "MonthlyReportEmail.html");
        HtmlTemplate = File.ReadAllText(path);
    }

    public MonthlyReportConsumer(
        IEmailSender emailSender,
        IPdfReportService pdfService,
        ILogger<MonthlyReportConsumer> logger)
    {
        _emailSender = emailSender;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MonthlyReportRequested> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Generating monthly report for project '{Project}' {Year}-{Month:D2} → {Email}",
            msg.ProjectName, msg.Year, msg.Month, msg.OwnerEmail);

        var pdf = _pdfService.GenerateMonthlyReport(msg);

        var monthName = GetMonthName(msg.Month);
        var subject = $"[WaveAccess] Отчет по проекту «{msg.ProjectName}» за {monthName} {msg.Year}";
        var html = BuildHtml(msg, monthName);
        var fileName = $"report_{msg.ProjectName}_{msg.Year}_{msg.Month:D2}.pdf";

        await _emailSender.SendWithAttachmentAsync(msg.OwnerEmail, subject, html, pdf, fileName);

        _logger.LogInformation(
            "Monthly report sent to {Email} for project '{Project}' ({Size} bytes)",
            msg.OwnerEmail, msg.ProjectName, pdf.Length);
    }

    private static string BuildHtml(MonthlyReportRequested msg, string monthName)
        => HtmlTemplate
            .Replace("{{FirstName}}", msg.OwnerFirstName)
            .Replace("{{ProjectName}}", msg.ProjectName)
            .Replace("{{MonthName}}", monthName)
            .Replace("{{Year}}", msg.Year.ToString())
            .Replace("{{TotalCreated}}", msg.TotalCreated.ToString())
            .Replace("{{SlaBreachCount}}", msg.SlaBreachCount.ToString())
            .Replace("{{AverageResolutionHours}}",
                msg.AverageResolutionHours > 0 ? $"{msg.AverageResolutionHours:F1} ч" : "—");

    private static string GetMonthName(int month) => month switch
    {
        1 => "январь",
        2 => "февраль",
        3 => "март",
        4 => "апрель",
        5 => "май",
        6 => "июнь",
        7 => "июль",
        8 => "август",
        9 => "сентябрь",
        10 => "октябрь",
        11 => "ноябрь",
        12 => "декабрь",
        _ => month.ToString()
    };
}
