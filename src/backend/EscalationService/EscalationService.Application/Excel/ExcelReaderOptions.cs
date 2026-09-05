namespace EscalationService.Application.Excel;

/// <summary>
/// Настройки чтения Excel файла
/// </summary>
public sealed class ExcelReaderOptions
{
    /// <summary>
    /// Номер строки с заголовками колонок
    /// </summary>
    public int HeaderRowNumber { get; set; } = 1;

    /// <summary>
    /// Номер строки, с которой начинается чтение данных
    /// </summary>
    public int DataStartRowNumber { get; set; } = 2;

    /// <summary>
    /// Максимальное количество строк для чтения (0 — без ограничений)
    /// </summary>
    public int MaxRows { get; set; } = 10_000;
}
