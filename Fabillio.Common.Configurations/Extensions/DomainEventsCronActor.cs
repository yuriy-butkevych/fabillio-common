using System;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using MediatR;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Fabillio.Common.Actors.CronActors;
using Fabillio.Common.Configurations.Entities;
using Fabillio.Common.Helpers.Extensions;

namespace Fabillio.Common.Configurations.Extensions;

public class DomainEventsCronActor : CronActorBase
{
    private readonly IDocumentStore _documentStore;
    private readonly IMediator _mediator;
    private const int ExpirationTimeDays = 7;

    public DomainEventsCronActor(ActorHost host, IDocumentStore documentStore, IMediator mediator)
        : base(host)
    {
        _documentStore = documentStore;
        _mediator = mediator;
    }

    protected override TimeSpan ExecutionInterval => TimeSpan.FromMinutes(15);

    public override async Task ExecuteJobAsync()
    {
        Console.WriteLine($"Domain Events cron actor started with Id {Id}");

        using var session = _documentStore.OpenAsyncSession();

        var domainEvents = await session
            .Query<DomainEvent>()
            .Where(x => !x.ProcessedOn.HasValue, false)
            .ToListAsync();

        var expiry = DateTime.UtcNow.AddDays(ExpirationTimeDays);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await _mediator.PublishDomainEventAsync(domainEvent.Event, Logger);

                domainEvent.ProcessedOn = DateTime.UtcNow;
                session.Advanced.GetMetadataFor(domainEvent)[
                    Raven.Client.Constants.Documents.Metadata.Expires
                ] = expiry;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    $"Domain event {domainEvent.Event} failed with message: {ex.Message}"
                );
            }
        }

        await session.SaveChangesAsync();
    }
}
