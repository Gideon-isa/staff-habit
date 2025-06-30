using System.Linq.Dynamic.Core;

namespace StaffHabit.Api.Services.Utility;

public static class OrderQueryExtension
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> queryable, string? orderByQueryString, string defaultOrderBy = "Id")
    {
        if (string.IsNullOrWhiteSpace(orderByQueryString))
        {
            return queryable.OrderBy(defaultOrderBy);
        }
        var orderQuery = OrderQueryBuilder.CreateOrderQuery<T>(orderByQueryString);
        if (string.IsNullOrWhiteSpace(orderQuery))
        {
            return queryable.OrderBy(defaultOrderBy);
        }
        return queryable.OrderBy(defaultOrderBy);
    }
}
