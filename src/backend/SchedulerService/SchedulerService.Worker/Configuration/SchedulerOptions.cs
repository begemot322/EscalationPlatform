namespace SchedulerService.Configuration;

public sealed class SchedulerOptions
{
    public const string SectionName = "Scheduler";

    /// <summary>Интервал проверки SLA в минутах (по умолчанию 60).</summary>
    public int CheckIntervalMinutes { get; set; } = 60;
}