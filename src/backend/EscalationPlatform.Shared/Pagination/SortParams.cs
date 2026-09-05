namespace EscalationPlatform.Shared.Pagination;

/// <summary>
/// Параметры сортировки.
/// </summary>
/// <param name="OrderBy">Поле для сортировки</param>
/// <param name="SortDirection">Направление сортировки</param>
public record SortParams(string? OrderBy = null, SortDirection? SortDirection = null);
