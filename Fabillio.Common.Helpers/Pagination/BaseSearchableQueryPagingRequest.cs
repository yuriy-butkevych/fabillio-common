namespace Fabillio.Common.Helpers.Pagination;
public class BaseSearchableQueryPagingRequest : BasePaginationRequest
{
    public string SearchingQuery { get; set; } = string.Empty;
}
