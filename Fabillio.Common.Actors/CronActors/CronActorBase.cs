using System;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;

namespace Fabillio.Common.Actors.CronActors;

public abstract class CronActorBase: Actor, ICronActor, IRemindable
{
    protected abstract TimeSpan ExecutionInterval { get; }

    protected CronActorBase(ActorHost host) : base(host)
    {
    }
    
    public virtual Task ScheduleJobsAsync()
    {
        Console.WriteLine($"Registering Reminders for the actor {Id}");
        
        return  RegisterReminderAsync(
            reminderName: "CronJobReminder",
            state: null,
            dueTime: TimeSpan.FromSeconds(0),
            period: ExecutionInterval
        );
    }

    public abstract Task ExecuteJobAsync();
    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        return ExecuteJobAsync();
    }
}