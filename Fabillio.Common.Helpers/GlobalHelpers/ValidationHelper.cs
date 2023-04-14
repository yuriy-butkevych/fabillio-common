using System;
using System.Linq;

namespace Fabillio.Common.Helpers.GlobalHelpers;

public static class ValidationHelper
{
    /// <summary>
    /// Can be used in AbstractValidator to be able to validate if field is sortable (in pair with RavenExtensions.OrderByField)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="sortField"></param>
    /// <returns></returns>
    public static bool IsPropertyInObject(Type type, string sortField)
    {
        if (type is null)
        {
            return false;
        }

        var fields = PropertyNameExtractor.GetPropertyNames(type);

        return fields.Length >= 1 && fields.Contains(sortField.ToLower());
    }
}