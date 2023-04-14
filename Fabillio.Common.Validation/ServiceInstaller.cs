using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Fabillio.Common.Validation;

public static class ServiceInstaller
{
    public static IServiceCollection AddValidation(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}
