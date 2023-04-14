using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Fabillio.Common.Configurations.Entities;
using Fabillio.Common.Events;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Helpers.Extensions;
using Fabillio.Common.Notifications.Contracts;

namespace Fabillio.Common.Configurations.DocumentNotifications;

public interface IDocumentStoreListener
{
    void OnAfterRavenDbSaveChanges(object sender, AfterSaveChangesEventArgs e);

    void OnBeforeStore(object sender, BeforeStoreEventArgs e);

    void OnBeforeDelete(object sender, BeforeDeleteEventArgs e);
}

internal class DocumentStoreListener : IDocumentStoreListener
{
    private const int ExpirationTimeDays = 7;
    private const string DomainEventsCollectionName = "domain-events";

    private readonly IServiceProvider _serviceProvider;

    public DocumentStoreListener(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void OnBeforeStore(object sender, BeforeStoreEventArgs e)
    {
        // Store notifications (domain events) - the old approach (will be removed)
        if (e.Entity is ISamvirkDomainEventsStore domainEventsStore)
        {
            StoreDomainEvents(domainEventsStore);
        }

        // Store events - the new approach
        if (e.Entity is IEventsDomainModel eventsDomainModel)
        {
            StoreOutboxEvents(eventsDomainModel, e.Session);
        }
    }

    public void OnBeforeDelete(object sender, BeforeDeleteEventArgs e)
    {
        // Store events - the new approach
        if (e.Entity is IEventsDomainModel eventsDomainModel)
        {
            StoreOutboxEvents(eventsDomainModel, e.Session);
        }
    }

    public void OnAfterRavenDbSaveChanges(object sender, AfterSaveChangesEventArgs e)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var mediator = serviceScope.ServiceProvider.GetService<IMediator>();
        var documentStore = serviceScope.ServiceProvider.GetService<IDocumentStore>();
        var loggerFactory = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(DocumentStoreListener));
        var eventPublisher = serviceScope.ServiceProvider.GetRequiredService<IEventPublisher>();

        switch (e.Session)
        {
            case DocumentSession session:
                PublishSessionOutboxEvents(session, eventPublisher);
                break;
            case AsyncDocumentSession asyncSession:
                Task.Run(() => PublishSessionOutboxEventsAsync(asyncSession, eventPublisher))
                    .Wait();
                break;
        }

        if (e.Entity is ISamvirkDomainEventsStore domainEventsStore)
        {
            PublishDomainEvents(domainEventsStore.DomainEvents, mediator, logger, documentStore);
        }
    }

    /// <summary>
    /// This method is used for the old approach using Domain Events
    /// </summary>
    /// <param name="domainEventsStore"></param>
    private void StoreDomainEvents(ISamvirkDomainEventsStore domainEventsStore)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var documentStore = serviceScope.ServiceProvider.GetRequiredService<IDocumentStore>();

        if (domainEventsStore.DomainEvents?.Any() != true)
        {
            return;
        }

        var domainEvents = domainEventsStore.DomainEvents
            .Select(
                domainEvent =>
                    new DomainEvent
                    {
                        DomainEventId = domainEvent.DomainEventId,
                        Event = domainEvent
                    }
            )
            .ToList();

        using var session = documentStore.OpenSession();

        foreach (var domainEvent in domainEvents)
        {
            session.Store(domainEvent);
        }

        session.SaveChanges();
    }

    /// <summary>
    /// This method is used to persist events in the database using the new approach
    /// </summary>
    private void StoreOutboxEvents(
        IEventsDomainModel domainModel,
        InMemoryDocumentSessionOperations session
    )
    {
        if (domainModel.Events == null)
        {
            return;
        }

        foreach (var domainModelEvent in domainModel.Events)
        {
            var outboxEvent = new OutboxEvent();

            outboxEvent.Create(domainModelEvent.GetEventTopic(), domainModelEvent);

            session.Store(outboxEvent, OutboxEvent.GetDocumentId(outboxEvent.EventId));
        }

        domainModel.Events.Clear();
    }

    private void PublishDomainEvents(
        List<ISamvirkDomainEvent> domainEvents,
        IMediator mediator,
        ILogger logger,
        IDocumentStore documentStore
    )
    {
        if (mediator == null)
            return;

        var expiry = DateTime.UtcNow.AddDays(ExpirationTimeDays);

        if (domainEvents?.Any() != true)
        {
            return;
        }

        ISamvirkDomainEvent currentDomainEvent = null;
        try
        {
            foreach (var domainEvent in domainEvents)
            {
                currentDomainEvent = domainEvent;
                Task.Run(() => mediator.PublishDomainEventAsync(domainEvent, logger)).Wait();

                using var session = documentStore.OpenSession();
                var documentEvent = session.Load<DomainEvent>(
                    $"{DomainEventsCollectionName}/{domainEvent.DomainEventId}"
                );
                documentEvent.ProcessedOn = DateTime.UtcNow;
                session.Advanced.GetMetadataFor(documentEvent)[
                    Raven.Client.Constants.Documents.Metadata.Expires
                ] = expiry;
                session.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                $"Domain event {currentDomainEvent} failed with message: {ex.Message}"
            );
            throw;
        }
        finally
        {
            domainEvents.Clear();
        }
    }

    private void PublishSessionOutboxEvents(DocumentSession session, IEventPublisher eventPublisher)
    {
        var outboxEvents = GetUnprocessedOutboxEvents(session.GetTrackedEntities().Values);

        if (!outboxEvents.Any())
        {
            return;
        }

        eventPublisher.PublishOutbox(outboxEvents, TimeSpan.FromDays(ExpirationTimeDays));
    }

    private async Task PublishSessionOutboxEventsAsync(
        AsyncDocumentSession session,
        IEventPublisher eventPublisher
    )
    {
        var outboxEvents = GetUnprocessedOutboxEvents(session.GetTrackedEntities().Values);

        if (!outboxEvents.Any())
        {
            return;
        }

        await eventPublisher.PublishOutboxAsync(
            outboxEvents,
            TimeSpan.FromDays(ExpirationTimeDays),
            CancellationToken.None
        );
    }

    private static List<OutboxEvent> GetUnprocessedOutboxEvents(ICollection<EntityInfo> entities)
    {
        return entities
            .Where(e => e.Entity is OutboxEvent)
            .Select(e => (OutboxEvent)e.Entity)
            .Where(e => !e.Published.HasValue)
            .ToList();
    }
}
