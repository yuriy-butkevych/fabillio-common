using System;
using Fabillio.Common.Services.Enums;

namespace Fabillio.Common.Services.LogEntities;

public class BackgroundCommand
{
    public static string GetDocumentId(Guid taskId) => $"backgroundCommands/{taskId}";

    public string Id => GetDocumentId(TaskId);

    public string TaskName { get; set; }
    public string FullName { get; set; }
    public Guid TaskId { get; set; }

    public BackgroundCommandStatus Status { get; set; }
    public BackgroundCommandExecutionType ExecutionType { get; set; }
    public int MaxExecutionDurationMinutes { get; set; }
    public DateTime Started { get; set; }
    public DateTime? Updated { get; set; }
    public string ExecutionDetails { get; set; }

    public void Create(
        Guid taskId,
        string taskName,
        string startedBy,
        BackgroundCommandExecutionType executionType,
        int executionMinutes
    )
    {
        TaskName = taskName;
        FullName = startedBy;
        TaskId = taskId;
        ExecutionType = executionType;
        MaxExecutionDurationMinutes = executionMinutes;

        Status = BackgroundCommandStatus.InProgress;
        Started = DateTime.UtcNow;
    }

    public void ChangeStatus(BackgroundCommandStatus status, string executionDetails = null)
    {
        Status = status;
        ExecutionDetails = executionDetails;
        Updated = DateTime.UtcNow;
    }
}
