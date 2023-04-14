using System;
using System.Collections.Generic;

namespace Fabillio.Common.Helpers.Implementations;

public abstract class DateTimeProvider
{
    private static DateTimeProvider _current = DefaultDateTimeProvider.Instance;

    public static DateTimeProvider Current
    {
        get { return _current; }
        set { _current = value ?? throw new ArgumentNullException(nameof(value)); }
    }

    public abstract DateTime UtcNow { get; }

    public static void ResetToDefault()
    {
        _current = DefaultDateTimeProvider.Instance;
    }
}
