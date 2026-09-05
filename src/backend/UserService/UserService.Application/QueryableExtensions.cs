using EscalationPlatform.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application;

/// <summary>
/// Общий класс расширений для работы с IQueryable
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Асинхронно преобразует запрос в пагинированный результат
    /// </summary>
    /// <typeparam name="T">Тип элементов запроса</typeparam>
    /// <param name="query">Исходный запрос</param>
    /// <param name="pageParams">Параметры пагинации (страница, размер страницы)</param>
    /// <param name="ct">Токен отмены операции</param>
    public static async Task<PagedResult<T>> ToPageAsync<T>(this IQueryable<T> query,
        PageParams? pageParams, CancellationToken ct)
    {
        var page = pageParams?.Page ?? 1;
        var pageSize = pageParams?.PageSize ?? 10;

        var totalCount = await query.CountAsync(ct);

        if (totalCount == 0)
            return new PagedResult<T>([], totalCount, page, pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}
