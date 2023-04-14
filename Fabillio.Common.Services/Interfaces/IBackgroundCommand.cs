using System;
using Fabillio.Common.Services.Enums;

namespace Fabillio.Common.Services.Interfaces;

/// <summary>
/// Extends IRequest to send a command to the background service
/// </summary>
public interface IBackgroundCommand
{
    Guid Id { get; set; }
    string Name { get; set; }
    BackgroundCommandExecutionType ExecutionType { get; set; }
    int MaxExecutionDurationMinutes { get; set; }
}

/// <summary>
/// Optional result if operation is successed and need to provide information about executed operation
/// </summary>
public interface IBackgroundCommandResult
{
    string ExecutionDetails { get; set; }
}
