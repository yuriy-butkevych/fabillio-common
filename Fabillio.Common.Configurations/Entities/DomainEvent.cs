using Fabillio.Common.Notifications.Contracts;
using System;

namespace Fabillio.Common.Configurations.Entities;

internal class DomainEvent
{
    public Guid DomainEventId { get; set; }

    public ISamvirkDomainEvent Event { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public string Id => GetDocumentId(DomainEventId);

    public static string GetDocumentId(Guid documentId)
    {
        return "domain-events/" + documentId;
    }
}
