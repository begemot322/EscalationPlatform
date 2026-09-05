namespace EscalationService.Application.Excel;

/// <summary>
/// Информация об ошибке в строке Excel файла
/// </summary>
public sealed class ExcelRowErrorModel
{
    /// <summary>
    /// Номер строки с ошибкой
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Название колонки, в котором найдена ошибка
    /// </summary>
    public string ColumnName { get; set; } = null!;
}
