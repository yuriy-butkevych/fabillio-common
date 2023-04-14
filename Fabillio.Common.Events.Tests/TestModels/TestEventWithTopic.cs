using System;
using Fabillio.Common.Events.Abstractions;

namespace Fabillio.Common.Events.Tests.TestModels;

[SamvirkEvent("test-topic")]
public class TestEventWithTopic: IEvent
{
    public DateTime When { get; } = DateTime.UtcNow;
}