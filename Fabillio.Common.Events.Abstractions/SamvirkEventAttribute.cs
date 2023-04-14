using System;

namespace Fabillio.Common.Events.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SamvirkEventAttribute : Attribute
{
    public string Topic { get; set; }

    public SamvirkEventAttribute() { }

    public SamvirkEventAttribute(string topic)
    {
        Topic = topic;
    }
}
