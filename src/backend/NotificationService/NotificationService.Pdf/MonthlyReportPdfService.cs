using EscalationPlatform.Shared.Messages;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NotificationService.Pdf;

/// <summary>
/// Генерирует ежемесячный PDF-отчет по эскалациям проекта с помощью QuestPDF.
/// </summary>
public sealed class MonthlyReportPdfService : IPdfReportService
{
    // Цвета в стиле WaveAccess
    private static readonly string AccentColor = "#1A73E8";
    private static readonly string HeaderBg = "#F8F9FA";
    private static readonly string TableHeaderBg = "#E8F0FE";
    private static readonly string DangerColor = "#D93025";
    private static readonly string MutedColor = "#5F6368";

    public byte[] GenerateMonthlyReport(MonthlyReportRequested report)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.8f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(h => ComposeHeader(h, report));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    // ── Шапка документа ─────────────────────────────────────────────────────

    private static void ComposeHeader(IContainer container, MonthlyReportRequested report)
    {
        var monthName = GetMonthName(report.Month);

        container
            .Background(AccentColor)
            .Padding(16)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item()
                        .Text("WaveAccess — Ежемесячный отчет")
                        .FontColor(Colors.White)
                        .FontSize(9)
                        .Italic();

                    col.Item()
                        .Text(report.ProjectName)
                        .FontColor(Colors.White)
                        .FontSize(18)
                        .Bold();

                    col.Item()
                        .Text($"{monthName} {report.Year}")
                        .FontColor(Colors.White)
                        .FontSize(11);
                });
            });
    }

    // ── Основное содержимое ─────────────────────────────────────────────────

    private static void ComposeContent(IContainer container, MonthlyReportRequested report)
    {
        container.PaddingTop(16).Column(col =>
        {
            // Сводные метрики
            col.Item().Element(c => ComposeSummaryCards(c, report));

            col.Item().PaddingTop(20).Row(row =>
            {
                // Левая колонка — по статусам
                row.RelativeItem().Element(c => ComposeBreakdownTable(
                    c, "По статусам", report.ByStatus));

                row.ConstantItem(16);

                // Правая колонка — по приоритетам
                row.RelativeItem().Element(c => ComposeBreakdownTable(
                    c, "По приоритетам", report.ByPriority));
            });
        });
    }

    // ── Карточки с ключевыми метриками ──────────────────────────────────────

    private static void ComposeSummaryCards(IContainer container, MonthlyReportRequested report)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c => SummaryCard(c, "Создано эскалаций",
                report.TotalCreated.ToString(), AccentColor));

            row.ConstantItem(12);

            row.RelativeItem().Element(c => SummaryCard(c, "Нарушений SLA",
                report.SlaBreachCount.ToString(),
                report.SlaBreachCount > 0 ? DangerColor : AccentColor));

            row.ConstantItem(12);

            row.RelativeItem().Element(c => SummaryCard(c, "Среднее время решения",
                report.AverageResolutionHours > 0
                    ? $"{report.AverageResolutionHours:F1} ч"
                    : "—",
                AccentColor));
        });
    }

    private static void SummaryCard(IContainer container, string label, string value, string color)
    {
        container
            .Background(HeaderBg)
            .Border(1)
            .BorderColor(color)
            .Padding(12)
            .Column(col =>
            {
                col.Item()
                    .Text(value)
                    .FontSize(22)
                    .Bold()
                    .FontColor(color);

                col.Item()
                    .PaddingTop(4)
                    .Text(label)
                    .FontSize(9)
                    .FontColor(MutedColor);
            });
    }

    // ── Таблица разбивки ────────────────────────────────────────────────────

    private static void ComposeBreakdownTable(
        IContainer container,
        string title,
        Dictionary<string, int> data)
    {
        container.Column(col =>
        {
            // Заголовок секции
            col.Item()
                .Background(TableHeaderBg)
                .Padding(8)
                .Text(title)
                .Bold()
                .FontSize(11)
                .FontColor(AccentColor);

            if (data.Count == 0)
            {
                col.Item()
                    .Padding(8)
                    .Text("Нет данных")
                    .FontColor(MutedColor)
                    .Italic();
                return;
            }

            var total = data.Values.Sum();

            // Строки таблицы
            foreach (var (key, count) in data.OrderByDescending(x => x.Value))
            {
                var pct = total > 0 ? (double)count / total * 100 : 0;

                col.Item()
                    .BorderBottom(1)
                    .BorderColor("#E0E0E0")
                    .PaddingVertical(6)
                    .PaddingHorizontal(8)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(key)
                            .FontColor("#202124");

                        row.AutoItem()
                            .Text($"{count}  ({pct:F0}%)")
                            .FontColor(MutedColor);
                    });
            }
        });
    }

    // ── Подвал ──────────────────────────────────────────────────────────────

    private static void ComposeFooter(IContainer container)
    {
        container
            .PaddingTop(8)
            .BorderTop(1)
            .BorderColor("#E0E0E0")
            .Row(row =>
            {
                row.RelativeItem()
                    .Text($"Сгенерировано: {DateTimeOffset.UtcNow:dd.MM.yyyy HH:mm} UTC")
                    .FontSize(8)
                    .FontColor(MutedColor);

                row.AutoItem()
                    .Text(text =>
                    {
                        text.Span("Стр. ").FontSize(8).FontColor(MutedColor);
                        text.CurrentPageNumber().FontSize(8).FontColor(MutedColor);
                        text.Span(" / ").FontSize(8).FontColor(MutedColor);
                        text.TotalPages().FontSize(8).FontColor(MutedColor);
                    });
            });
    }

    // ── Вспомогательное ─────────────────────────────────────────────────────

    private static string GetMonthName(int month) => month switch
    {
        1 => "Январь",
        2 => "Февраль",
        3 => "Март",
        4 => "Апрель",
        5 => "Май",
        6 => "Июнь",
        7 => "Июль",
        8 => "Август",
        9 => "Сентябрь",
        10 => "Октябрь",
        11 => "Ноябрь",
        12 => "Декабрь",
        _ => month.ToString()
    };
}
