using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Extensions;
using Fabillio.Common.Services.Enums;
using Fabillio.Common.Services.Interfaces;
using Fabillio.Common.Services.Storages;

namespace Fabillio.Common.Services.BackgroundServices;

public class BackgroundCommandsService : BackgroundService
{
    private readonly BackgroundCommandsStorage _storage;

    private readonly ConcurrentQueue<IBackgroundCommand> _sequentialCommands;
    private readonly ConcurrentBag<IBackgroundCommand> _concurrentCommands;
    private readonly ManualResetEventSlim _sequentialCommandState;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundCommandsService> _logger;

    public BackgroundCommandsService(
        BackgroundCommandsStorage storage,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundCommandsService> logger
    )
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _serviceScopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _sequentialCommandState = new ManualResetEventSlim(false);
        _cancellationTokenSource = new CancellationTokenSource();
        _concurrentCommands = new ConcurrentBag<IBackgroundCommand>();
        _sequentialCommands = new ConcurrentQueue<IBackgroundCommand>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                IDictionary<
                    BackgroundCommandExecutionType,
                    IEnumerable<IBackgroundCommand>
                > typesCommandsPairs = await _storage.TakeCommandsByTypes(cancellationToken);

                foreach (
                    (
                        BackgroundCommandExecutionType type,
                        IEnumerable<IBackgroundCommand> commands
                    ) in typesCommandsPairs
                )
                {
                    foreach (IBackgroundCommand command in commands)
                    {
                        if (command.ExecutionType is BackgroundCommandExecutionType.Concurrently)
                        {
                            _ = ExecuteTaskAsync(command, cancellationToken);
                            _concurrentCommands.Add(command);
                        }
                        else if (
                            command.ExecutionType is BackgroundCommandExecutionType.Sequentially
                        )
                        {
                            _sequentialCommands.Enqueue(command);
                        }
                        else
                        {
                            if (
                                command.MaxExecutionDurationMinutes is not 0
                                && command.MaxExecutionDurationMinutes <= 10
                            )
                            {
                                _ = ExecuteTaskAsync(command, cancellationToken);
                                _concurrentCommands.Add(command);
                            }
                            else
                            {
                                _sequentialCommands.Enqueue(command);
                            }
                        }
                    }
                }

                if (!_sequentialCommands.IsEmpty && !_sequentialCommandState.IsSet)
                {
                    if (_sequentialCommands.TryDequeue(out var longTask))
                    {
                        _ = ExecuteTaskAsync(longTask, cancellationToken);
                        _sequentialCommandState.Set();
                    }
                }

                _logger.LogInformation(
                    $"{nameof(BackgroundCommandsService)} Total groups: {_storage.BackgroundTaskTypesCount}, Total commands: {_storage.TotalCommandsCount}"
                );
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                $"{nameof(BackgroundCommandsService)} exception occured {exception?.Message}"
            );
        }
    }

    private async Task ExecuteTaskAsync(
        IBackgroundCommand command,
        CancellationToken cancellationToken
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var taskService = scope.ServiceProvider.GetRequiredService<IBackgroundCommandResolver>();

        var executionTime = command.MaxExecutionDurationMinutes is not 0
            ? TimeSpan.FromMinutes(command.MaxExecutionDurationMinutes)
            : TimeSpan.FromMilliseconds(-1);

        var taskCancellationSource = new CancellationTokenSource(executionTime);

        try
        {
            var result = await mediator
                .Send(command, cancellationToken)
                .WithCancellation(taskCancellationSource.Token);

            string details = string.Empty;

            if (result is IBackgroundCommandResult commandResult)
            {
                details = commandResult.ExecutionDetails;
            }

            await taskService.CloseCommand(command.Id, BackgroundCommandStatus.Done, details);
        }
        catch (OperationCanceledException cancellationException)
            when (taskCancellationSource.Token.IsCancellationRequested) // Normal cancellation using linked token
        {
            _logger.LogInformation(
                $"{nameof(BackgroundCommandsService)} task cancelled {cancellationException}",
                cancellationException
            );
            await taskService.CloseCommand(
                command.Id,
                BackgroundCommandStatus.CancelledByCancellationToken,
                $"{command.Name} cancelled due to exceeding the maximum allowed time to execute."
            );
        }
        catch (OperationCanceledException cancellationException)
            when (cancellationToken.IsCancellationRequested) // Unhandled cancellation
        {
            _logger.LogError(
                cancellationException,
                $"{nameof(BackgroundCommandsService)} task cancelled {cancellationException}"
            );
            await taskService.CloseCommand(
                command.Id,
                BackgroundCommandStatus.CancelledByCancellationToken,
                $"{command.Name} cancelled due to exceeding the maximum allowed time to execute."
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                $"{nameof(BackgroundCommandsService)} exception occured {exception?.Message}"
            );
            await taskService.CloseCommand(
                command.Id,
                BackgroundCommandStatus.TerminatedDueToError,
                $"{command.Name} cancelled due to an error: {exception.Message}"
            );
        }
        finally
        {
            // to close tasks in case of server soft shutdown
            if (command.ExecutionType == BackgroundCommandExecutionType.Concurrently)
            {
                foreach (var item in _concurrentCommands)
                {
                    if (_concurrentCommands.TryTake(out var removedItem) && command == removedItem)
                    {
                        // item removed
                    }
                }
            }
            else
            {
                if (command.ExecutionType == BackgroundCommandExecutionType.Sequentially)
                {
                    // release the queue (sequentialCommands)
                    _sequentialCommandState.Reset();

                    if (!_sequentialCommands.IsEmpty)
                        _storage.ReleaseSignal();
                }
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var taskService = scope.ServiceProvider.GetRequiredService<IBackgroundCommandResolver>();
        foreach (var commandToCancel in _sequentialCommands)
        {
            if (_sequentialCommands.TryDequeue(out var command))
            {
                await taskService.CloseCommand(
                    command.Id,
                    BackgroundCommandStatus.CancelledByServer,
                    $"{command.Name} cancelled because the server is shutting down."
                );
            }
        }

        foreach (var commandToCancel in _concurrentCommands)
        {
            if (_concurrentCommands.TryTake(out var command))
            {
                await taskService.CloseCommand(
                    command.Id,
                    BackgroundCommandStatus.CancelledByServer,
                    $"{command.Name} cancelled because the server is shutting down."
                );
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
