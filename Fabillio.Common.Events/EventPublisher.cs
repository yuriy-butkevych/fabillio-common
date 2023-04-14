using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands.Batches;
using Raven.Client.Documents.Operations;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Helpers.Implementations;

namespace Fabillio.Common.Events;

public class EventPublisher : IEventPublisher
{
    private readonly DaprClient _daprClient;
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<EventPublisher> _logger;
    public const string PubSub = "pubsub";
    private const int _expirationTimeDays = 7;

    public EventPublisher(
        DaprClient daprClient,
        IDocumentStore documentStore,
        ILogger<EventPublisher> logger
    )
    {
        _daprClient = daprClient;
        _documentStore = documentStore;
        _logger = logger;
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        Task.Run(() => PublishAsync(@event, CancellationToken.None)).Wait();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken)
        where T : IEvent
    {
        var topicName = @event.GetEventTopic();

        await _daprClient.PublishEventAsync(PubSub, topicName, @event, cancellationToken);
    }

    public void PublishOutbox(List<OutboxEvent> outboxEvents, TimeSpan? expiresAfter)
    {
        Task.Run(() => PublishOutboxAsync(outboxEvents, expiresAfter, CancellationToken.None))
            .Wait();
    }

    public async Task PublishOutboxAsync(
        List<OutboxEvent> outboxEvents,
        TimeSpan? expiresAfter,
        CancellationToken cancellationToken
    )
    {
        using var documentSession = _documentStore.OpenAsyncSession();
        var expiry = expiresAfter.HasValue
            ? DateTimeProvider.Current.UtcNow.Add(expiresAfter.Value)
            : (DateTime?)null;

        var processedOutboxEventsIds = new List<string>();

        foreach (var outboxEvent in outboxEvents)
        {
            try
            {
                await PublishEvent(outboxEvent.Event, cancellationToken);

                processedOutboxEventsIds.Add(outboxEvent.Id);
            }
            catch (Exception exc)
            {
                _logger.LogError(
                    exc,
                    $"Publishing outbox event to topic {outboxEvent.Topic} failed with message: {exc.Message}"
                );
            }
        }

        if (processedOutboxEventsIds.Any())
        {
            var command = new BatchPatchCommandData(
                processedOutboxEventsIds,
                new PatchRequest
                {
                    Script =
                        $"this.Published = '{DateTimeProvider.Current.UtcNow:s}';"
                        + $"this['@metadata']['@expires'] = '{expiry:s}'"
                },
                null
            );

            documentSession.Advanced.Defer(command);

            await documentSession.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task PublishWithOutbox<T>(T @event, CancellationToken cancellationToken)
        where T : IEvent
    {
        using var documentSession = _documentStore.OpenAsyncSession();
        var topic = @event.GetEventTopic();
        var outboxEvent = new OutboxEvent();

        outboxEvent.Create(topic, @event);

        await documentSession.StoreAsync(outboxEvent, cancellationToken);

        try
        {
            await PublishEvent(@event, cancellationToken);

            outboxEvent.MarkAsPublished();

            documentSession.Advanced.GetMetadataFor(outboxEvent)[
                Constants.Documents.Metadata.Expires
            ] = DateTimeProvider.Current.UtcNow.AddDays(_expirationTimeDays);
        }
        catch (Exception exc)
        {
            _logger.Log(LogLevel.Error, exc, $"Failed to publish Dapr Event {outboxEvent.EventId}");
        }
        finally
        {
            await documentSession.SaveChangesAsync(cancellationToken);
        }
    }

    private Task PublishEvent(IEvent @event, CancellationToken cancellationToken)
    {
        MethodInfo publishMethod = typeof(DaprClient)
            .GetMethods()
            .Where(x => x.Name == nameof(DaprClient.PublishEventAsync))
            .First(x => x.IsGenericMethod && x.GetParameters().Length == 4);
        MethodInfo genericPublishMethod = publishMethod.MakeGenericMethod(@event.GetType());
        object[] parametersArray = { PubSub, @event.GetEventTopic(), @event, cancellationToken };

        return (Task)genericPublishMethod.Invoke(_daprClient, parametersArray);
    }
}
