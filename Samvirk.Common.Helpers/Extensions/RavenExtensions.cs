using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Raven.Client.Documents.Linq;
using Fabillio.Common.Helpers.Models;

namespace Fabillio.Common.Helpers.Extensions;

/// <summary>
/// Suggestion: use in a bundle with
/// For request - BasePaginationRequest or BaseSearchablePaginationRequest
/// For query - RavenExtensions IRavenQueryable<TSource> WithPagination<TSource>
/// For result - PaginationResult<TSource>;
/// </summary>
public static class RavenExtensions
{
    public static IRavenQueryable<TSource> WithPagination<TSource>(
        this IRavenQueryable<TSource> source,
        int page,
        int size,
        Action<SortingParams> sortingParams = null
    )
    {
        if (sortingParams is null)
        {
            return page <= 0 || size <= 0 ? source : source.Skip(size * (page - 1)).Take(size);
        }

        var defaultParams = new SortingParams();
        sortingParams.Invoke(defaultParams);

        if (!string.IsNullOrWhiteSpace(defaultParams.SortField))
        {
            source = source.OrderByField(defaultParams.SortField, defaultParams.IsDescending);
        }

        return page <= 0 || size <= 0 ? source : source.Skip(size * (page - 1)).Take(size);
    }

    public static int GetCurrentPage(this int requestedPageNumber, int pageSize, int totalItemCount)
    {
        var pageNumber = requestedPageNumber is 0 ? 1 : requestedPageNumber;

        var amountOfPages = pageSize is 0
            ? 1
            : (totalItemCount / pageSize) + (totalItemCount % pageSize > 0 ? 1 : 0);

        return pageNumber > amountOfPages ? amountOfPages : pageNumber;
    }

    public static IRavenQueryable<TSource> OrderByField<TSource>(
        this IRavenQueryable<TSource> source,
        string sortField,
        bool isDescending
    )
    {
        var type = typeof(TSource);

        var propertyInfo = type.GetProperty(
            sortField,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
        );

        if (propertyInfo == null)
        {
            throw new ArgumentException(
                $"Could not find a property named '{sortField}' on type '{type.FullName}'."
            );
        }

        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
        var orderByExp = Expression.Lambda(propertyAccess, parameter);

        var method = isDescending ? "OrderByDescending" : "OrderBy";

        var types = new Type[] { type, propertyInfo.PropertyType };
        var resultExp = Expression.Call(
            typeof(Queryable),
            method,
            types,
            source.Expression,
            Expression.Quote(orderByExp)
        );

        return (IRavenQueryable<TSource>)source.Provider.CreateQuery<TSource>(resultExp);
    }
}
