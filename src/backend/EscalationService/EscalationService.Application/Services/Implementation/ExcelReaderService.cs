using ClosedXML.Excel;
using EscalationService.Application.Excel;
using EscalationService.Application.Services.Interfaces;

namespace EscalationService.Application.Services.Implementation;

/// <summary>
/// Реализация сервиса чтения Excel файлов на базе ClosedXML
/// </summary>
public sealed class ExcelReaderService : IExcelReaderService
{
    public ExcelReadResult Read(Stream fileStream, List<ColumnDefinition> columns, ExcelReaderOptions? options = null)
    {
        options ??= new ExcelReaderOptions();
        var result = new ExcelReadResult();

        try
        {
            using var workbook = new XLWorkbook(fileStream);

            if (workbook.Worksheets.Count == 0)
                return ExcelReadResult.Failure("Файл не содержит листов.");

            var worksheet = workbook.Worksheet(1);

            // 1. Маппинг заголовков
            var headerRow = worksheet.Row(options.HeaderRowNumber);
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var name = cell.GetString().Trim();
                if (!string.IsNullOrEmpty(name))
                    headerMap[name] = cell.Address.ColumnNumber;
            }

            // 2. Проверяем наличие обязательных колонок
            var columnMap = new Dictionary<string, int>();
            foreach (var colDef in columns)
            {
                if (headerMap.TryGetValue(colDef.Name, out var colIndex))
                    columnMap[colDef.Name] = colIndex;
                else if (colDef.IsRequired)
                    result.StructureErrors.Add(new ExcelRowErrorModel { RowNumber = 1, ColumnName = colDef.Name });
                else
                    columnMap[colDef.Name] = -1;
            }

            if (result.StructureErrors.Count != 0)
                return result;

            // 3. Словари для проверки уникальности
            var uniqueMaps = columns
                .Where(c => c.IsUnique)
                .ToDictionary(
                    c => c.Name,
                    _ => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));

            var rowsData = new List<Dictionary<string, object?>>();

            // 4. Читаем строки с данными
            foreach (var row in worksheet.RowsUsed())
            {
                var rowNum = row.RowNumber();
                if (rowNum < options.DataStartRowNumber) continue;
                if (options.MaxRows > 0 && rowsData.Count >= options.MaxRows) break;

                var rowData = new Dictionary<string, object?> { ["RowNumber"] = rowNum };
                var rowHasError = false;

                foreach (var colDef in columns)
                {
                    var colIndex = columnMap[colDef.Name];
                    object? cellValue = null;

                    if (colIndex > 0)
                    {
                        var cell = row.Cell(colIndex);
                        if (!cell.IsEmpty())
                            cellValue = cell.Value;
                    }

                    var isEmpty = cellValue == null || (cellValue is string s && string.IsNullOrWhiteSpace(s));

                    // 5. Проверка обязательности
                    if (colDef.IsRequired && isEmpty)
                    {
                        result.EmptyValueErrors.Add(new ExcelRowErrorModel { RowNumber = rowNum, ColumnName = colDef.Name });
                        rowHasError = true;
                        continue;
                    }

                    // 6. Проверка уникальности
                    if (colDef.IsUnique && !isEmpty)
                    {
                        var valueStr = cellValue!.ToString()!.Trim();
                        var uniqueMap = uniqueMaps[colDef.Name];
                        if (uniqueMap.ContainsKey(valueStr))
                        {
                            result.DuplicateValueErrors.Add(new ExcelRowErrorModel { RowNumber = rowNum, ColumnName = colDef.Name });
                            rowHasError = true;
                        }
                        else
                            uniqueMap[valueStr] = rowNum;
                    }

                    rowData[colDef.Name] = isEmpty ? null : cellValue;
                }

                if (!rowHasError)
                    rowsData.Add(rowData);
            }

            if (result.EmptyValueErrors.Count > 0 || result.DuplicateValueErrors.Count > 0)
            {
                result.IsSuccess = false;
                result.Rows.Clear();
            }
            else
            {
                result.IsSuccess = true;
                result.Rows = rowsData;
            }

            return result;
        }
        catch (Exception ex)
        {
            return ExcelReadResult.Failure($"Критическая ошибка чтения файла: {ex.Message}");
        }
    }
}
