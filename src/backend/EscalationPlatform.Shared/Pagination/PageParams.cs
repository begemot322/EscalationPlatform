namespace EscalationPlatform.Shared.Pagination;

/// <summary>
/// Параметры пагинации.
/// </summary>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Количество элементов на странице</param>
public record PageParams(int Page = 1, int PageSize = 10);