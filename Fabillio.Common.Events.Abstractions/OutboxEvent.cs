using System;
using Fabillio.Common.Helpers.Implementations;

namespace Fabillio.Common.Events.Abstractions;

public class OutboxEvent
{
    public Guid EventId { get; private set; }
    public IEvent Event { get; private set; }
    public string Topic { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Published { get; private set; }
    public string Id => GetDocumentId(EventId);

    public static string GetDocumentId(Guid documentId)
    {
        return "outbox-events/" + documentId;
    }

    public void Create(string topic, IEvent @event)
    {
        EventId = Guid.NewGuid();
        Event = @event;
        Topic = topic;
        Created = DateTimeProvider.Current.UtcNow;
    }

    public void MarkAsPublished()
    {
        Published = DateTimeProvider.Current.UtcNow;
    }
}
