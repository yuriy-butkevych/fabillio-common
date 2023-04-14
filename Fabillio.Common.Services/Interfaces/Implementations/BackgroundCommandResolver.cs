using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Documents;
using Fabillio.Common.Helpers.Extensions;
using Fabillio.Common.Services.Enums;
using Fabillio.Common.Services.LogEntities;
using Fabillio.Common.Services.Models;
using Fabillio.Common.Services.Options;
using Fabillio.Common.Services.Storages;

namespace Fabillio.Common.Services.Interfaces.Implementations;

public class BackgroundCommandResolver : IBackgroundCommandResolver
{
    private readonly BackgroundServiceOptions _backgroundServiceOptions;

    private readonly IDocumentStore _documentStore;
    private readonly BackgroundCommandsStorage _commandStorage;
    private readonly ILogger<BackgroundCommandResolver> _logger;

    public BackgroundCommandResolver(
        IDocumentStore documentStore,
        BackgroundCommandsStorage commandStorage,
        ILogger<BackgroundCommandResolver> logger,
        BackgroundServiceOptions backgroundServiceOptions
    )
    {
        _documentStore = documentStore;
        _commandStorage = commandStorage;
        _backgroundServiceOptions = backgroundServiceOptions;
        _logger = logger;
    }

    public async Task<BackgroundCommandResult> TryCreateCommand(
        string taskName,
        string fullName,
        IBackgroundCommand task,
        bool forceCreation = false
    )
    {
        if (task is null)
        {
            _logger.LogInformation($"Incoming task: {taskName}, Runned by: {fullName} is null");
            return new BackgroundCommandResult();
        }

        using var documentSession = _documentStore.OpenAsyncSession();

        var previousStatus = await documentSession
            .Query<BackgroundCommand>()
            .Where(task => task.TaskName == taskName)
            .OrderByDescending(task => task.Started)
            .FirstOrDefaultAsync();

        if (
            previousStatus is null
            || previousStatus.Status != BackgroundCommandStatus.InProgress
            || forceCreation
        )
        {
            var newTask = new BackgroundCommand();

            newTask.Create(
                task.Id,
                taskName,
                fullName,
                task.ExecutionType,
                task.MaxExecutionDurationMinutes
            );

            var expirationDate = DateTime.UtcNow.AddDays(
                _backgroundServiceOptions.LogEntitiesExpirationDays
            );
            await documentSession.StoreAsync(newTask);
            documentSession.Advanced.GetMetadataFor(newTask)[Constants.Documents.Metadata.Expires] =
                expirationDate;
            await documentSession.SaveChangesAsync();

            _commandStorage.AddCommand(task);

            newTask.Started.IsTimePassed(
                _backgroundServiceOptions.MinutesToProcessBackgroundCommand,
                out int minutesLeft
            );

            return new BackgroundCommandResult(
                Available: true,
                Status: newTask.Status,
                Started: newTask.Started,
                FullName: newTask.FullName,
                MinutesLeft: minutesLeft,
                TaskId: newTask.TaskId
            );
        }
        else
        {
            if (
                !previousStatus.Started.IsTimePassed(
                    _backgroundServiceOptions.MinutesToProcessBackgroundCommand,
                    out int minutesLeft
                )
            )
            {
                return new BackgroundCommandResult(
                    Available: false,
                    Status: previousStatus.Status,
                    Started: previousStatus.Started,
                    FullName: previousStatus.FullName,
                    MinutesLeft: minutesLeft
                );
            }
            else
            {
                previousStatus.ChangeStatus(
                    BackgroundCommandStatus.CancelledByNextTask,
                    "The task has canceled because the completion time has elapsed"
                );
                await documentSession.SaveChangesAsync();

                return await TryCreateCommand(taskName, fullName, task);
            }
        }
    }

    public async Task CloseCommand(
        Guid taskId,
        BackgroundCommandStatus? status = null,
        string details = null
    )
    {
        using var documentSession = _documentStore.OpenAsyncSession();

        var task = await documentSession.LoadAsync<BackgroundCommand>(
            BackgroundCommand.GetDocumentId(taskId)
        );

        if (task is null)
        {
            _logger.LogError($"Incoming task: {taskId}, not found");
            return;
        }

        if (status is null)
        {
            task.ChangeStatus(BackgroundCommandStatus.Done, details);
        }
        else
        {
            task.ChangeStatus(status.Value, details);
        }

        await documentSession.SaveChangesAsync();
    }
}
