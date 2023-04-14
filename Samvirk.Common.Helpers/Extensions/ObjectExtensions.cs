using System.Reflection;

namespace Fabillio.Common.Helpers.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Use in Oslofjord and BCC Portal client to avoid white spaces
    /// </summary>
    /// <param name="obj"></param>
    public static void TrimStringProperties(this object obj)
    {
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (
                property.PropertyType == typeof(string)
                && property.CanRead
                && property.CanWrite
                && property.GetIndexParameters().Length == 0
            )
            {
                var currentValue = (string)property.GetValue(obj);
                if (currentValue != null)
                {
                    var trimmedValue = currentValue.Trim();
                    property.SetValue(obj, trimmedValue);
                }
            }
        }
    }
}
