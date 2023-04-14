using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Fabillio.Common.Exceptions.ExceptionHandlers;

namespace Fabillio.Common.Exceptions;

public static class ServiceInstaller
{
    private static readonly Type _handlerOpenType = typeof(IExceptionHandler<>);
    private static readonly Type _handlerDefaultType = _handlerOpenType.MakeGenericType(
        typeof(Exception)
    );

    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services,
        Assembly[] assemblies
    )
    {
        var assembliesContainingHandlers = assemblies
            .Concat(new[] { typeof(IExceptionHandler<>).Assembly })
            .ToArray();
        return services.RegisterAllGenericTypes(
            assembliesContainingHandlers,
            typeof(IExceptionHandler<>),
            ServiceLifetime.Transient
        );
    }

    public static IApplicationBuilder UseExceptionHandling(
        this IApplicationBuilder app,
        IServiceScopeFactory scopeFactory
    )
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature != null)
                {
                    dynamic exception = exceptionHandlerFeature.Error;

                    using var exceptionHandlerScope = scopeFactory.CreateScope();

                    IServiceProvider serviceProvider = exceptionHandlerScope.ServiceProvider;

                    Type[] typeArgs = { exception.GetType() };

                    dynamic handler =
                        serviceProvider.GetService(_handlerOpenType.MakeGenericType(typeArgs))
                        ?? serviceProvider.GetRequiredService(_handlerDefaultType);

                    await handler.HandleException(exception, context);
                }
            });
        });

        return app;
    }

    private static IServiceCollection RegisterAllGenericTypes(
        this IServiceCollection services,
        Assembly[] assemblies,
        Type type,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
    {
        Type[] types = assemblies.SelectMany(a => a.GetTypes()).ToArray();

        types
            .Where(
                item =>
                    item.GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .Any(i => i.GetGenericTypeDefinition() == type)
                    && !item.IsAbstract
                    && !item.IsInterface
            )
            .ToList()
            .ForEach(assignedType =>
            {
                var serviceType = assignedType
                    .GetInterfaces()
                    .First(i => i.GetGenericTypeDefinition() == type);
                services.AddScoped(serviceType, assignedType);
                services.Add(new ServiceDescriptor(serviceType, assignedType, lifetime));
            });

        return services;
    }
}
