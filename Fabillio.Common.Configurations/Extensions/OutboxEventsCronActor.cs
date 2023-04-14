using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Raven.Client.Documents;
using Fabillio.Common.Actors.CronActors;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Helpers.Implementations;

namespace Fabillio.Common.Configurations.Extensions;

public class OutboxEventsCronActor : CronActorBase
{
    private readonly IDocumentStore _documentStore;
    private readonly IEventPublisher _eventPublisher;
    private const int ExpirationTimeDays = 7;

    public OutboxEventsCronActor(
        ActorHost host,
        IDocumentStore documentStore,
        IEventPublisher eventPublisher
    ) : base(host)
    {
        _documentStore = documentStore;
        _eventPublisher = eventPublisher;
    }

    protected override TimeSpan ExecutionInterval => TimeSpan.FromMinutes(15);

    public override async Task ExecuteJobAsync()
    {
        Console.WriteLine($"Outbox Events cron actor started with Id {Id}");

        using var session = _documentStore.OpenAsyncSession();
        var minuteAgo = DateTimeProvider.Current.UtcNow.AddMinutes(-1);
        var outboxEvents = await session
            .Query<OutboxEvent>()
            .Where(x => !x.Published.HasValue, false)
            .Where(x => x.Created < minuteAgo, false)
            .ToListAsync();

        await _eventPublisher.PublishOutboxAsync(
            outboxEvents,
            TimeSpan.FromDays(ExpirationTimeDays),
            CancellationToken.None
        );
    }
}
