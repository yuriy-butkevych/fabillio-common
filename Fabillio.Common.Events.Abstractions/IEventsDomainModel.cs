using System.Collections.Generic;

namespace Fabillio.Common.Events.Abstractions;

public interface IEventsDomainModel
{
    List<IEvent> Events { get; set; }
}
