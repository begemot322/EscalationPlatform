using EscalationPlatform.Shared.Messages;

namespace NotificationService.Pdf;

/// <summary>
/// Генерирует PDF-документы для уведомлений.
/// </summary>
public interface IPdfReportService
{
    /// <summary>
    /// Генерирует ежемесячный отчет по проекту в виде PDF.
    /// </summary>
    /// <param name="report">Данные отчета из RabbitMQ.</param>
    /// <returns>Байты готового PDF-документа.</returns>
    byte[] GenerateMonthlyReport(MonthlyReportRequested report);
}
