namespace Fabillio.Common.Helpers.Models;

public class SortingParams
{
    public string SortField { get; private set; }
    public bool IsDescending { get; private set; }

    public void SortBy(string field, bool? isDescending = null)
    {
        SortField = field;
        IsDescending = isDescending ?? false;
    }
}