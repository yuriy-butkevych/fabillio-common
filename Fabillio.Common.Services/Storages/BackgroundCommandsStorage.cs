using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Fabillio.Common.Services.Enums;
using Fabillio.Common.Services.Interfaces;

namespace Fabillio.Common.Services.Storages;

public class BackgroundCommandsStorage
{
    private readonly ConcurrentDictionary<
        BackgroundCommandExecutionType,
        ConcurrentBag<IBackgroundCommand>
    > _backgroundCommands;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly ILogger<BackgroundCommandsStorage> _logger;

    public BackgroundCommandsStorage(ILogger<BackgroundCommandsStorage> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _semaphoreSlim = new(2);
        _backgroundCommands = new();
    }

    public int BackgroundTaskTypesCount => _backgroundCommands.Count;
    public int TotalCommandsCount => _backgroundCommands.Values.Count;

    public void AddCommand(IBackgroundCommand command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var type = command.ExecutionType;

        if (!_backgroundCommands.TryGetValue(type, out var commands))
        {
            commands = new ConcurrentBag<IBackgroundCommand>();
            _backgroundCommands[type] = commands;
        }

        commands.Add(command);

        _semaphoreSlim.Release();
    }

    public async Task<
        IDictionary<BackgroundCommandExecutionType, IEnumerable<IBackgroundCommand>>
    > TakeCommandsByTypes(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        var commandsToExecute =
            new Dictionary<BackgroundCommandExecutionType, IEnumerable<IBackgroundCommand>>();

        if (
            _backgroundCommands.TryGetValue(
                BackgroundCommandExecutionType.Concurrently,
                out var shortTasks
            )
        )
        {
            var toExecute = new List<IBackgroundCommand>();

            for (var index = 0; index < shortTasks.Count; index++)
            {
                if (shortTasks.TryTake(out var item))
                {
                    toExecute.Add(item);
                }
            }

            commandsToExecute.Add(BackgroundCommandExecutionType.Concurrently, toExecute);
        }

        if (
            _backgroundCommands.TryGetValue(
                BackgroundCommandExecutionType.Sequentially,
                out var longTasks
            )
        )
        {
            var toExecute = new List<IBackgroundCommand>();

            for (var index = 0; index < longTasks.Count; index++)
            {
                if (longTasks.TryTake(out var item))
                {
                    toExecute.Add(item);
                }
            }

            commandsToExecute.Add(BackgroundCommandExecutionType.Sequentially, toExecute);
        }

        _logger.LogInformation(
            $"{nameof(BackgroundCommandsStorage)} commands removed: {commandsToExecute.Count}"
        );
        return commandsToExecute;
    }

    public void ReleaseSignal() => _semaphoreSlim.Release();
}
