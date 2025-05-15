using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;

namespace EventTestTask.Infrastructure.Extensions;

public static class EventExtension
{
    public static IQueryable<Event> Filter(this IQueryable<Event> query, EventFilter filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(f => f.StartDate == filter.StartDate);

        if (filter.EndDate.HasValue)
            query = query.Where(f => f.EndDate == filter.EndDate);

        if (!string.IsNullOrEmpty(filter.Location))
            query = query.Where(f => f.Location == filter.Location);

        if (filter.Category.HasValue)
            query = query.Where(f => f.Category == filter.Category);

        return query;
    }
}