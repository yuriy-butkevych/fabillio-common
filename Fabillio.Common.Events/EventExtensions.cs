using System;
using System.Linq;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Helpers.Extensions;

namespace Fabillio.Common.Events;

public static class EventExtensions
{
    public static string GetEventTopic(this IEvent @event)
    {
        var samvirkEventAttribute = (SamvirkEventAttribute)
            Attribute
                .GetCustomAttributes(@event.GetType(), typeof(SamvirkEventAttribute))
                .FirstOrDefault();

        return samvirkEventAttribute?.Topic ?? @event.GetType().Name.ToKebabCase();
    }

    public static string GetEventTopic(this Type eventType)
    {
        if (!eventType.GetInterfaces().Contains(typeof(IEvent)))
        {
            throw new ArgumentException();
        }

        var samvirkEventAttribute = (SamvirkEventAttribute)
            Attribute
                .GetCustomAttributes(eventType, typeof(SamvirkEventAttribute))
                .FirstOrDefault();

        return samvirkEventAttribute?.Topic ?? eventType.Name.ToKebabCase();
    }
}
