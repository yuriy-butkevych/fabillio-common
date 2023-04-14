using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Fabillio.Common.Services.BackgroundServices;
using Fabillio.Common.Services.Interfaces;
using Fabillio.Common.Services.Interfaces.Implementations;
using Fabillio.Common.Services.Options;
using Fabillio.Common.Services.Storages;

namespace Fabillio.Common.Services;

public static class ServiceInstaller
{
    public static IServiceCollection AddSamvirkCommonServices(
        this IServiceCollection services,
        Action<BackgroundServiceOptions> options = null
    )
    {
        services.AddSingleton<BackgroundCommandsStorage>();

        services.AddHostedService<BackgroundCommandsService>();

        var backgroundServiceOptions = new BackgroundServiceOptions();
        options?.Invoke(backgroundServiceOptions);

        services.AddTransient<IBackgroundCommandResolver, BackgroundCommandResolver>(
            serviceFactory =>
                new BackgroundCommandResolver(
                    serviceFactory.GetRequiredService<IDocumentStore>(),
                    serviceFactory.GetRequiredService<BackgroundCommandsStorage>(),
                    serviceFactory.GetRequiredService<ILogger<BackgroundCommandResolver>>(),
                    backgroundServiceOptions
                )
        );

        return services;
    }
}
