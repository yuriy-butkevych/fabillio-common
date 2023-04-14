using System.Threading.Tasks;
using Dapr.Actors;

namespace Fabillio.Common.Actors.CronActors;

public interface ICronActor: IActor
{
    Task ScheduleJobsAsync();

    Task ExecuteJobAsync();
}