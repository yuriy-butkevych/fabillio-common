using System;

namespace Fabillio.Common.Helpers.Extensions;

public static class SafetyExtensions
{
    public static void IfNotNull<T>(this T instance, Action<T> action)
        where T: class
    {
        if (instance != null)
        {
            action(instance);
        }
    }
    
    public static TResult IfNotNull<T, TResult>(this T instance, Func<T, TResult> action, TResult nullResult = default)
        where T: class
    {
        return instance != null ? action(instance) : nullResult;
    }
}