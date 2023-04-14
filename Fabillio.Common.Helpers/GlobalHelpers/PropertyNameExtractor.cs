using System;
using System.Linq;
using System.Reflection;

namespace Fabillio.Common.Helpers.GlobalHelpers;

public static class PropertyNameExtractor
{
    public static string[] GetPropertyNames(Type type)
    {
        if (type is null)
            return Array.Empty<string>();

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(HasPublicGetter)
            .Select(p => p.Name.ToLower())
            .ToArray();
    }

    private static bool HasPublicGetter(PropertyInfo property)
        => property.GetGetMethod() is not null;
}