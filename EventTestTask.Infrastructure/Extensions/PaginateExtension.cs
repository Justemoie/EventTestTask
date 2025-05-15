using EventTestTask.Core.Models.Pagination;
using Microsoft.EntityFrameworkCore;

namespace EventTestTask.Infrastructure.Extensions;

public static class PaginateExtension
{
    public static async Task<PageResult<T>> ToPage<T>(
        this IQueryable<T> query, 
        PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var count = await query.CountAsync(cancellationToken);
        
        if(count == 0)
            return new PageResult<T>([], 0);
        
        var page = pageParams.Page ?? 1;
        var pageSize = pageParams.PageSize ?? 10;
        
        var skip = (page - 1) * pageSize;
        var result = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return new PageResult<T>(result, count);
    }
}