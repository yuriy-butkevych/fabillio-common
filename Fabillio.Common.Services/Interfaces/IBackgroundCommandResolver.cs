using System;
using System.Threading.Tasks;
using Fabillio.Common.Services.Enums;
using Fabillio.Common.Services.Models;

namespace Fabillio.Common.Services.Interfaces;

public interface IBackgroundCommandResolver
{
    Task<BackgroundCommandResult> TryCreateCommand(
        string taskName,
        string fullName,
        IBackgroundCommand task,
        bool forceCreation = false
    );
    Task CloseCommand(
        Guid taskId,
        BackgroundCommandStatus? longTaskStatus = null,
        string details = null
    );
}
