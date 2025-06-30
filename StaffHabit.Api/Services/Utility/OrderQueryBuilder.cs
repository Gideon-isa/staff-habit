using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace StaffHabit.Api.Services.Utility;

public static class OrderQueryBuilder
{
    public static string CreateOrderQuery<T>(string orderByQueryString)
    {
        var oderParams = orderByQueryString.Trim().Split(',');
        var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();

        foreach (var param in oderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                continue;
            }
            var propertyFromQueryName = param.Split(' ')[0];
            var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
            if (objectProperty is null)
            {
                continue;
            }
            var direction = param.EndsWith(" desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
            var queryString = $"{objectProperty.Name} {direction},";
            orderQueryBuilder.Append(queryString);
        }
        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
        return orderQuery;
    }
}
