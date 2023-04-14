using Microsoft.Extensions.DependencyInjection;
using Fabillio.Common.Actors.CronActors;

namespace Fabillio.Common.Actors;

public static class ServiceInstaller
{
    public static IServiceCollection AddCronActors(this IServiceCollection services)
    {
        services.AddSingleton<ICronActorsScheduler, CronActorsScheduler>();

        return services;
    }
}