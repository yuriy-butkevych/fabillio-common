using System;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Fabillio.Common.Clients.Oslofjord.Authentication;
using Fabillio.Common.Clients.Oslofjord.Configurations;
using Fabillio.Common.Clients.Oslofjord.Interfaces;
using Fabillio.Common.Clients.Oslofjord.Interfaces.Implementations;

namespace Fabillio.Common.Clients.Oslofjord.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOslofjordApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeSpan? timeout = null
    )
    {
        var options = new OslofjordOptions();
        configuration.GetSection("Oslofjord").Bind(options);

        services.AddSingleton(options);

        services.AddSingleton<OslofjordAuthentication>();
        services.AddTransient<OslofjordAuthorizationHandler>();

        services
            .AddHttpClient(
                OslofjordSamvirkClient.HttpClientName,
                client =>
                {
                    client.BaseAddress = new Uri(options.ApiBaseUrl);
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        HeaderNames.ContentType,
                        MediaTypeNames.Application.Json
                    );
                    if (timeout is not null)
                        client.Timeout = timeout.Value;
                }
            )
            .AddHttpMessageHandler<OslofjordAuthorizationHandler>();

        services.AddSingleton<IOslofjordSamvirkService, OslofjordSamvirkClient>();

        return services;
    }
}
