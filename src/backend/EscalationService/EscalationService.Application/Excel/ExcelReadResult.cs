namespace EscalationService.Application.Excel;

/// <summary>
/// Результат чтения Excel файла
/// </summary>
public sealed class ExcelReadResult
{
    /// <summary>
    /// Успешно ли выполнено чтение и валидация
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Ошибки структуры файла (отсутствуют обязательные колонки)
    /// </summary>
    public List<ExcelRowErrorModel> StructureErrors { get; set; } = [];

    /// <summary>
    /// Ошибки: пустое значение в обязательной колонке
    /// </summary>
    public List<ExcelRowErrorModel> EmptyValueErrors { get; set; } = [];

    /// <summary>
    /// Ошибки: дублирующееся значение в уникальной колонке
    /// </summary>
    public List<ExcelRowErrorModel> DuplicateValueErrors { get; set; } = [];

    /// <summary>
    /// Распарсенные строки. Заполняется только если IsSuccess = true.
    /// Каждый словарь — одна строка; ключ — имя колонки, значение — данные ячейки.
    /// Дополнительно всегда содержит ключ "RowNumber".
    /// </summary>
    public List<Dictionary<string, object?>> Rows { get; set; } = [];

    /// <summary>
    /// Быстрое создание результата с критической ошибкой
    /// </summary>
    public static ExcelReadResult Failure(string error) => new()
    {
        IsSuccess = false,
        StructureErrors = [new ExcelRowErrorModel { RowNumber = 0, ColumnName = error }]
    };
}
