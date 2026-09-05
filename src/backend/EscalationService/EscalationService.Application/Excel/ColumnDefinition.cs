namespace EscalationService.Application.Excel;

/// <summary>
/// Описание колонки для парсинга из Excel файла
/// </summary>
public sealed class ColumnDefinition
{
    /// <summary>
    /// Название колонки в Excel файле
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Признак обязательности — отсутствие или пустое значение вызовет ошибку
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Должны ли значения в колонке быть уникальными в пределах файла
    /// </summary>
    public bool IsUnique { get; }

    public ColumnDefinition(string name, bool isRequired = false, bool isUnique = false)
    {
        Name = name;
        IsRequired = isRequired;
        IsUnique = isUnique;
    }
}
