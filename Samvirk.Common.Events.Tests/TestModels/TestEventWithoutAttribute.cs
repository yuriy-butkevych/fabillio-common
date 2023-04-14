using System;
using Fabillio.Common.Events.Abstractions;

namespace Fabillio.Common.Events.Tests.TestModels;

public class TestEventWithoutAttribute : IEvent
{
    public DateTime When { get; } = DateTime.UtcNow;
}
