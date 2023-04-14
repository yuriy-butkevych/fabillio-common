namespace Fabillio.Common.Helpers.Pagination;
/// <summary>
/// Suggestion: use in a bundle with 
/// For request - BasePaginationRequest or BaseSearchablePaginationRequest
/// For query - RavenExtensions IRavenQueryable<TSource> WithPagination<TSource> 
/// For result - PaginationResult<TSource>;
/// </summary>
public class PaginationResult<TSource> where TSource : class
{
    /// <summary>
    /// Based on paging request with default values
    /// </summary>
    /// <param name="pageSize">item(s) per page</param>
    /// <param name="currentPage">page which is displayed in result from back-end if range is out of bound; from front-end if page exists</param>
    /// <param name="totalItems">back-end value, count of all items</param>
    /// <param name="data">results</param>
    public PaginationResult(int pageSize, int currentPage, int totalItems, TSource data)
    {
        PageSize = pageSize;
        CurrentPage = currentPage is 0 ? 1 : currentPage;
        TotalItems = totalItems;
        Data = data;
    }

    public int TotalPages => PageSize is 0 ? 1 : (TotalItems / PageSize) is 0 ? 1 : TotalItems / PageSize;
    public int PageSize { get; }
    public int CurrentPage { get; }
    public int TotalItems { get; }
    public TSource Data { get; }
}
