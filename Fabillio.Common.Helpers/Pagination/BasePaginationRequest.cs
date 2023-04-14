namespace Fabillio.Common.Helpers.Pagination;
/// <summary>
/// Suggestion: use in a bundle with 
/// For request - BasePaginationRequest or BaseSearchablePaginationRequest
/// For query - RavenExtensions IRavenQueryable<TSource> WithPagination<TSource> 
/// For result - PaginationResult<TSource>;
/// </summary>
public class BasePaginationRequest
{
    /// <summary>
    /// 0 - grab all
    /// </summary>
    public int PageSize { get; set; } = 0;
    /// <summary>
    /// 0 - grab all
    /// </summary>
    public int PageNumber { get; set; } = 0;
}
