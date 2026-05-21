namespace EscalationPlatform.Shared.Pagination;

/// <summary>
/// Представляет результат пагинации
/// </summary>
/// <param name="Items">Список элементов текущей страницы</param>
/// <param name="TotalCount">Общее количество элементов</param>
/// <param name="Page">Номер текущей страницы</param>
/// <param name="PageSize">Размер страницы</param>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}