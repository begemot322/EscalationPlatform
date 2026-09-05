namespace SchedulerService.Configuration;

public sealed class SchedulerOptions
{
    public const string SectionName = "Scheduler";

    /// <summary>Интервал проверки SLA в минутах (по умолчанию 60).</summary>
    public int CheckIntervalMinutes { get; set; } = 60;

    /// <summary>Включить SlaMonitorWorker. При false воркер стартует, но сразу выходит.</summary>
    public bool SlaMonitorEnabled { get; set; }

    /// <summary>Включить MonthlyReportWorker. При false воркер стартует, но сразу выходит.</summary>
    public bool MonthlyReportEnabled { get; set; }

    /// <summary>
    /// Запустить MonthlyReportWorker сразу при старте, не дожидаясь 1-го числа.
    /// Удобно для локального тестирования. В проде держать false.
    /// </summary>
    public bool MonthlyReportRunImmediately { get; set; }
}