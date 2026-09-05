using EscalationService.Application.Excel;

namespace EscalationService.Application.Services.Interfaces;

/// <summary>
/// Сервис чтения и валидации данных из Excel файлов
/// </summary>
public interface IExcelReaderService
{
    /// <summary>
    /// Читает данные из Excel файла, проверяет структуру и валидирует строки
    /// </summary>
    /// <param name="fileStream">Поток файла .xlsx</param>
    /// <param name="columns">Описание ожидаемых колонок</param>
    /// <param name="options">Параметры чтения</param>
    ExcelReadResult Read(Stream fileStream, List<ColumnDefinition> columns, ExcelReaderOptions? options = null);
}
