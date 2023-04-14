using System;
using MediatR;

namespace Fabillio.Common.Notifications.Contracts;

public interface ISamvirkDomainEvent : INotification
{
    Guid DomainEventId { get; }

    DateTime When { get; }
}
