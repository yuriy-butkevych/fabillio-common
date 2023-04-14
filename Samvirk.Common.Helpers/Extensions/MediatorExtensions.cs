using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Fabillio.Common.Notifications.Contracts;

namespace Fabillio.Common.Helpers.Extensions;

public static class MediatorExtensions
{
    private static readonly TimeSpan _domainEventTimeLimit = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Publishes domain event and logs a warning if it takes more than 30 seconds
    /// </summary>
    public static async Task PublishDomainEventAsync(
        this IMediator mediator,
        ISamvirkDomainEvent domainEvent,
        ILogger logger,
        CancellationToken cancellationToken = default
    )
    {
        var timer = Stopwatch.StartNew();

        await mediator.Publish(domainEvent, cancellationToken);

        timer.Stop();

        if (timer.Elapsed > _domainEventTimeLimit)
        {
            var warning =
                $"Processing of the domain event {domainEvent.DomainEventId} of type {domainEvent.GetType().Name} took {timer.Elapsed.Seconds}s";
            logger.LogCritical(warning);
        }
    }
}
