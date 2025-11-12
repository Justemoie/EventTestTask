using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;

namespace EventTestTask.Infrastructure.Extensions;

public static class EventExtension
{
    public static IQueryable<Event> Filter(this IQueryable<Event> query, EventFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(term)
            );
        }
        
        if (filter.StartDate.HasValue)
        {
            var start = filter.StartDate.Value.Date; // 00:00:00
            query = query.Where(e => e.StartDate >= start);
        }
        
        if (filter.EndDate.HasValue)
        {
            var end = filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(e => e.EndDate <= end);
        }
        
        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var loc = filter.Location.Trim();
            query = query.Where(e => e.Location.Contains(loc));
        }
        
        if (filter.Category.HasValue)
        {
            query = query.Where(e => e.Category == filter.Category.Value);
        }

        return query;
    }
}