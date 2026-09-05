using System.Linq.Expressions;
using EscalationPlatform.Shared.Pagination;
using EscalationService.Application.DTOs.Escalation;
using EscalationService.Domain.Entities;

namespace EscalationService.Application.Extensions;

/// <summary>
/// Класс расширений для работы с запросами эскалаций
/// </summary>
public static class EscalationExtensions
{
    public static IQueryable<Escalation> Filter(this IQueryable<Escalation> query, EscalationFilter? filter)
    {
        if (filter is null) return query;

        if (filter.Status.HasValue)
            query = query.Where(e => e.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(e => e.Priority == filter.Priority.Value);

        if (filter.Type.HasValue)
            query = query.Where(e => e.Type == filter.Type.Value);

        if (filter.AuthorId.HasValue)
            query = query.Where(e => e.AuthorId == filter.AuthorId.Value);

        if (filter.ProjectId.HasValue)
            query = query.Where(e => e.ProjectId == filter.ProjectId.Value);

        if (filter.ResponsibleUserId.HasValue)
            query = filter.ResponsibleUserRole.HasValue
                ? query.Where(e => e.Participants.Any(p =>
                    p.UserId == filter.ResponsibleUserId.Value &&
                    p.Role == filter.ResponsibleUserRole.Value))
                : query.Where(e => e.Participants.Any(p =>
                    p.UserId == filter.ResponsibleUserId.Value));

        if (filter.CreatedAfter.HasValue)
            query = query.Where(e => e.CreatedAt >= filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            query = query.Where(e => e.CreatedAt <= filter.CreatedBefore.Value);

        return query;
    }

    public static IQueryable<Escalation> Sort(this IQueryable<Escalation> query, SortParams? sortParams)
    {
        if (sortParams is null)
            return query.OrderByDescending(e => e.CreatedAt);

        return sortParams.SortDirection == SortDirection.Descending
            ? query.OrderByDescending(GetKeySelector(sortParams.OrderBy))
            : query.OrderBy(GetKeySelector(sortParams.OrderBy));
    }

    private static Expression<Func<Escalation, object>> GetKeySelector(string? orderBy) =>
        orderBy switch
        {
            nameof(Escalation.Name) => e => e.Name,
            nameof(Escalation.Status) => e => e.Status,
            nameof(Escalation.CreatedAt) => e => e.CreatedAt,
            nameof(Escalation.UpdatedAt) => e => e.UpdatedAt,
            nameof(Escalation.AuthorId) => e => e.AuthorId,
            _ => e => e.CreatedAt
        };
}
