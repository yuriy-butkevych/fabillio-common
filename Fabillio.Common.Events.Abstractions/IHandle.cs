using System.Threading;
using System.Threading.Tasks;

namespace Fabillio.Common.Events.Abstractions;

public interface IHandle { }

public interface IHandle<in T> : IHandle where T : IEvent
{
    Task Handle(T @event, CancellationToken cancellationToken);
}
