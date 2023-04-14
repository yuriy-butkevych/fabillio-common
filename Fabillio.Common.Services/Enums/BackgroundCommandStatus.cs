namespace Fabillio.Common.Services.Enums;

public enum BackgroundCommandStatus
{
    CancelledByNextTask = 0,
    CancelledByCancellationToken = 1,
    CancelledByServer = 2,
    InProgress = 3,
    Done = 4,
    TerminatedDueToError = 5
}
