using System;

namespace Fabillio.Common.Helpers.Implementations;

public class DefaultDateTimeProvider : DateTimeProvider
{
    private static readonly Lazy<DefaultDateTimeProvider> _instance = new();

    public static DefaultDateTimeProvider Instance => _instance.Value;

    public override DateTime UtcNow => DateTime.UtcNow;
}
