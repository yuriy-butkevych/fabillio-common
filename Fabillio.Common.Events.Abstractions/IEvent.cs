using System;

namespace Fabillio.Common.Events.Abstractions;

public interface IEvent
{
    DateTime When { get; }
}
