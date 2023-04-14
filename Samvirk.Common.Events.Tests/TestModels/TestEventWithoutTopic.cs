using System;
using Fabillio.Common.Events.Abstractions;

namespace Fabillio.Common.Events.Tests.TestModels;

[SamvirkEvent]
public class TestEventWithoutTopic : IEvent
{
    public DateTime When { get; } = DateTime.UtcNow;
}
