using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fabillio.Common.Events.Abstractions;

public interface IEventPublisher
{
    void Publish<T>(T @event) where T : IEvent;

    Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : IEvent;

    void PublishOutbox(List<OutboxEvent> outboxEvents, TimeSpan? expiresAfter);

    Task PublishOutboxAsync(
        List<OutboxEvent> outboxEvents,
        TimeSpan? expiresAfter,
        CancellationToken cancellationToken
    );

    Task PublishWithOutbox<T>(T @event, CancellationToken cancellationToken) where T : IEvent;
}
