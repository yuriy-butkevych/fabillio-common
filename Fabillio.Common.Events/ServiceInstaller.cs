using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Dapr.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Messaging;

namespace Fabillio.Common.Events;

public static class ServiceInstaller
{
    private static readonly string _daprEventsRoutePrefix = "/dapr-events";
    private static readonly Type _handlerOpenType = typeof(IHandle<>);

    public static IServiceCollection AddEvents(
        this IServiceCollection services,
        Assembly[] assemblies
    )
    {
        var types = assemblies.SelectMany(a => a.GetTypes()).ToArray();

        types
            .Where(
                item =>
                    item.GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .Any(i => i.GetGenericTypeDefinition() == _handlerOpenType)
                    && !item.IsAbstract
                    && !item.IsInterface
            )
            .ToList()
            .ForEach(assignedType =>
            {
                var interfaces = assignedType
                    .GetInterfaces()
                    .Where(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == _handlerOpenType
                    );

                foreach (var i in interfaces)
                {
                    services.Add(new ServiceDescriptor(i, assignedType, ServiceLifetime.Transient));
                }

                services.Add(
                    new ServiceDescriptor(typeof(IHandle), assignedType, ServiceLifetime.Transient)
                );
            });

        services.AddTransient<IEventPublisher, EventPublisher>();

        return services;
    }

    public static IEndpointConventionBuilder MapSamvirkEventsSubscriptions(
        this IEndpointRouteBuilder endpoints,
        IServiceScopeFactory scopeFactory
    )
    {
        return CreateSubscribeEndPoint(endpoints, scopeFactory);
    }

    public static IEndpointConventionBuilder MapSamvirkEventsSubscriptions(
        this IEndpointRouteBuilder endpoints,
        IServiceScopeFactory scopeFactory,
        SubscribeOptions options
    )
    {
        return CreateSubscribeEndPoint(endpoints, scopeFactory, options);
    }

    private static IEndpointConventionBuilder CreateSubscribeEndPoint(
        IEndpointRouteBuilder endpoints,
        IServiceScopeFactory scopeFactory,
        SubscribeOptions options = null
    )
    {
        if (endpoints is null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        var eventSubscriptions = CreateEventsEndpoints(endpoints, scopeFactory);

        return endpoints.MapGet(
            "dapr/subscribe",
            async context =>
            {
                var logger = context.RequestServices
                    .GetService<ILoggerFactory>()!
                    .CreateLogger("DaprTopicSubscription");
                var dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
                var subscriptions = dataSource.Endpoints
                    .OfType<RouteEndpoint>()
                    .Where(
                        e =>
                            e.Metadata.GetOrderedMetadata<ITopicMetadata>().Any(t => t.Name != null)
                    ) // only endpoints which have TopicAttribute with not null Name.
                    .SelectMany(e =>
                    {
                        var topicMetadata = e.Metadata.GetOrderedMetadata<ITopicMetadata>();
                        var originalTopicMetadata =
                            e.Metadata.GetOrderedMetadata<IOriginalTopicMetadata>();
                        var bulkSubscribeMetadata =
                            e.Metadata.GetOrderedMetadata<IBulkSubscribeMetadata>();

                        var subs =
                            new List<(string PubsubName, string Name, string DeadLetterTopic, bool? EnableRawPayload, string Match, int Priority, Dictionary<
                                    string,
                                    string[]
                                > OriginalTopicMetadata, string MetadataSeparator, RoutePattern RoutePattern, DaprTopicBulkSubscribe bulkSubscribe)>();

                        for (int i = 0; i < topicMetadata.Count(); i++)
                        {
                            DaprTopicBulkSubscribe bulkSubscribe = null;

                            foreach (var bulkSubscribeAttr in bulkSubscribeMetadata)
                            {
                                if (bulkSubscribeAttr.TopicName != topicMetadata[i].Name)
                                {
                                    continue;
                                }

                                bulkSubscribe = new DaprTopicBulkSubscribe
                                {
                                    Enabled = true,
                                    MaxMessagesCount = bulkSubscribeAttr.MaxMessagesCount,
                                    MaxAwaitDurationMs = bulkSubscribeAttr.MaxAwaitDurationMs
                                };
                                break;
                            }

                            subs.Add(
                                (
                                    topicMetadata[i].PubsubName,
                                    topicMetadata[i].Name,
                                    (topicMetadata[i] as IDeadLetterTopicMetadata)?.DeadLetterTopic,
                                    (topicMetadata[i] as IRawTopicMetadata)?.EnableRawPayload,
                                    topicMetadata[i].Match,
                                    topicMetadata[i].Priority,
                                    originalTopicMetadata
                                        .Where(
                                            m =>
                                                (
                                                    topicMetadata[i] as IOwnedOriginalTopicMetadata
                                                )?.OwnedMetadatas?.Any(o => o.Equals(m.Id)) == true
                                                || string.IsNullOrEmpty(m.Id)
                                        )
                                        .GroupBy(c => c.Name)
                                        .ToDictionary(
                                            m => m.Key,
                                            m => m.Select(c => c.Value).Distinct().ToArray()
                                        ),
                                    (
                                        topicMetadata[i] as IOwnedOriginalTopicMetadata
                                    )?.MetadataSeparator,
                                    e.RoutePattern,
                                    bulkSubscribe
                                )
                            );
                        }

                        return subs;
                    })
                    .Distinct()
                    .GroupBy(e => new { e.PubsubName, e.Name })
                    .Select(e => e.OrderBy(e => e.Priority))
                    .Select(e =>
                    {
                        var first = e.First();
                        var rawPayload = e.Any(e => e.EnableRawPayload.GetValueOrDefault());
                        var metadataSeparator =
                            e.FirstOrDefault(
                                e => !string.IsNullOrEmpty(e.MetadataSeparator)
                            ).MetadataSeparator ?? ",";
                        var rules = e.Where(e => !string.IsNullOrEmpty(e.Match)).ToList();
                        var defaultRoutes = e.Where(e => string.IsNullOrEmpty(e.Match))
                            .Select(e => RoutePatternToString(e.RoutePattern))
                            .ToList();
                        var defaultRoute = defaultRoutes.FirstOrDefault();

                        //multiple identical names. use comma separation.
                        var metadata = new Metadata(
                            e.SelectMany(c => c.OriginalTopicMetadata)
                                .GroupBy(c => c.Key)
                                .ToDictionary(
                                    c => c.Key,
                                    c =>
                                        string.Join(
                                            metadataSeparator,
                                            c.SelectMany(c => c.Value).Distinct()
                                        )
                                )
                        );
                        if (rawPayload || options?.EnableRawPayload is true)
                        {
                            metadata.Add(Metadata.RawPayload, "true");
                        }

                        if (defaultRoutes.Count > 1)
                        {
                            logger.LogError(
                                "A default subscription to topic {name} on pubsub {pubsub} already exists.",
                                first.Name,
                                first.PubsubName
                            );
                        }

                        var duplicatePriorities = rules
                            .GroupBy(e => e.Priority)
                            .Where(g => g.Count() > 1)
                            .ToDictionary(x => x.Key, y => y.Count());

                        foreach (var entry in duplicatePriorities)
                        {
                            logger.LogError(
                                "A subscription to topic {name} on pubsub {pubsub} has duplicate priorities for {priority}: found {count} occurrences.",
                                first.Name,
                                first.PubsubName,
                                entry.Key,
                                entry.Value
                            );
                        }

                        var subscription = new Subscription
                        {
                            Topic = first.Name,
                            PubsubName = first.PubsubName,
                            Metadata = metadata.Count > 0 ? metadata : null,
                            BulkSubscribe = first.bulkSubscribe
                        };

                        if (first.DeadLetterTopic != null)
                        {
                            subscription.DeadLetterTopic = first.DeadLetterTopic;
                        }

                        // Use the V2 routing rules structure
                        if (rules.Count > 0)
                        {
                            subscription.Routes = new Routes
                            {
                                Rules = rules
                                    .Select(
                                        e =>
                                            new Rule
                                            {
                                                Match = e.Match,
                                                Path = RoutePatternToString(e.RoutePattern),
                                            }
                                    )
                                    .ToList(),
                                Default = defaultRoute,
                            };
                        }
                        // Use the V1 structure for backward compatibility.
                        else
                        {
                            subscription.Route = defaultRoute;
                        }

                        return subscription;
                    })
                    .OrderBy(e => (e.PubsubName, e.Topic));

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        subscriptions.Concat(eventSubscriptions),
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        }
                    )
                );
            }
        );
    }

    private static List<Subscription> CreateEventsEndpoints(
        IEndpointRouteBuilder endpoints,
        IServiceScopeFactory scopeFactory
    )
    {
        var eventHandlersSubscriptions = new List<Subscription>();
        using var exceptionHandlerScope = scopeFactory.CreateScope();
        var serviceProvider = exceptionHandlerScope.ServiceProvider;

        var handlers = serviceProvider.GetServices(typeof(IHandle));
        foreach (var handler in handlers)
        {
            if (handler == null)
            {
                continue;
            }

            var eventTypes = handler
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.IsAssignableFrom(typeof(IHandle<IEvent>)))
                .SelectMany(i => i.GetGenericArguments());

            foreach (var eventType in eventTypes)
            {
                var topicName = eventType.GetEventTopic();

                endpoints
                    .MapPost(
                        $"{_daprEventsRoutePrefix}/{topicName}",
                        async (
                            [FromServices] IServiceProvider services,
                            [FromServices] IHttpContextAccessor httpContextAccessor
                        ) =>
                        {
                            var httpContext = httpContextAccessor.HttpContext;
                            var request = httpContext.Request;
                            var @event = await request.ReadFromJsonAsync(eventType);
                            Type[] typeArgs = { eventType };
                            var eventHandlerType = _handlerOpenType.MakeGenericType(typeArgs);
                            var eventHandlers = services.GetServices(eventHandlerType);

                            MethodInfo methodInfo = eventHandlerType.GetMethod(
                                nameof(IHandle<IEvent>.Handle)
                            );
                            object[] parametersArray = { @event, CancellationToken.None };
                            var tasks = eventHandlers
                                .Select(
                                    eventHandler =>
                                        eventHandler != null
                                            ? (Task)methodInfo.Invoke(eventHandler, parametersArray)
                                            : Task.FromResult(0)
                                )
                                .ToList();

                            await Task.WhenAll(tasks);
                        }
                    )
                    .Add(endpointBuilder =>
                    {
                        endpointBuilder.Metadata.Add(new AllowAnonymousAttribute());
                        endpointBuilder.Metadata.Add(new DaprEndpointAttribute());
                    });

                eventHandlersSubscriptions.Add(
                    new Subscription
                    {
                        Topic = topicName,
                        PubsubName = EventPublisher.PubSub,
                        Route = $"{_daprEventsRoutePrefix}/{topicName}"
                    }
                );
            }
        }

        return eventHandlersSubscriptions;
    }

    private static string RoutePatternToString(RoutePattern routePattern)
    {
        return string.Join(
            "/",
            routePattern.PathSegments.Select(
                segment =>
                    string.Concat(
                        segment.Parts.Cast<RoutePatternLiteralPart>().Select(part => part.Content)
                    )
            )
        );
    }
}
