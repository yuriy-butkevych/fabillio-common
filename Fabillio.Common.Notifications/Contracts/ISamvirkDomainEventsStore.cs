using System.Collections.Generic;

namespace Fabillio.Common.Notifications.Contracts;

public interface ISamvirkDomainEventsStore
{
    List<ISamvirkDomainEvent> DomainEvents { get; }
}
