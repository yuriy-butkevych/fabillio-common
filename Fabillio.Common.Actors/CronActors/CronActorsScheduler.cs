using System;
using System.Linq;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fabillio.Common.Actors.CronActors;

public interface ICronActorsScheduler
{
    Task ScheduleCronActors();
}

internal class CronActorsScheduler : ICronActorsScheduler
{
    private static readonly TimeSpan _actorRuntimeInitializationDelay = TimeSpan.FromMinutes(1);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CronActorsScheduler> _logger;

    public CronActorsScheduler(
        IServiceProvider serviceProvider,
        ILogger<CronActorsScheduler> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ScheduleCronActors()
    {
        await Task.Delay(_actorRuntimeInitializationDelay);

        var actorRuntime = _serviceProvider.GetRequiredService<ActorRuntime>();
        var actorProxyFactory = _serviceProvider.GetRequiredService<IActorProxyFactory>();

        var registeredCronActors = actorRuntime.RegisteredActors.Where(
            x => x.Type.InterfaceTypes.Contains(typeof(ICronActor))
        );

        foreach (var registration in registeredCronActors)
        {
            var actorTypeName = registration.Type.ImplementationType.Name;
            var actorId = new ActorId(actorTypeName);
            var cronActor = actorProxyFactory.CreateActorProxy<ICronActor>(actorId, actorTypeName);

            _logger.LogInformation(message: "Scheduling actor {0}.", actorTypeName);
            await cronActor.ScheduleJobsAsync();
            _logger.LogInformation(message: "Scheduled actor {0}.", actorTypeName);
        }
    }
}
