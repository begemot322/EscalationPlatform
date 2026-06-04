namespace EscalationService.Application.DTOs.Project;

/// <summary>
/// Результат импорта проектов из Excel файла
/// </summary>
public sealed record ProjectImportResult(
    int Imported,
    List<string> SkippedDuplicateCodes
);
