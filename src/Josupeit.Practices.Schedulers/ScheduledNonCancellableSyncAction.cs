// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledNonCancellableSyncAction : IScheduledAction
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Action _action;
    private readonly ExecutionContext _executionContext;

    public ScheduledNonCancellableSyncAction(Action action, ExecutionContext? executionContext)
    {
        _action = action;
        _executionContext = executionContext ?? Singletons.DefaultExecutionContext;
    }

    bool IScheduled.IsCancellationRequested => false;

    public Task Promise => _taskCompletionSource.Task;

    public void Decline()
    {
        _taskCompletionSource.TrySetException(
            new ExecutionDeclinedException());
    }

    public void Execute(CancellationToken executionCancellationToken)
    {
        if (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource.TrySetException(
                new ExecutionDeclinedException());

            throw new OperationCanceledException(executionCancellationToken);
        }

        try
        {
            ExecutionContext.Run(_executionContext, static self => ((ScheduledNonCancellableSyncAction)self!)._action(), state: this);

            _taskCompletionSource.TrySetResult(null);
        }
        catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource.TrySetException(
                new ExecutionDeclinedException(ex));

            throw;
        }
        catch (OperationCanceledException)
        {
            _taskCompletionSource.TrySetCanceled();
        }
        catch (Exception ex)
        {
            _taskCompletionSource.TrySetException(ex);
        }
    }

    public Task ExecuteAsync(CancellationToken executionCancellationToken)
    {
        if (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource.TrySetException(
                new ExecutionDeclinedException());

            return Task.FromCanceled(executionCancellationToken);
        }

        try
        {
            ExecutionContext.Run(_executionContext, static self => ((ScheduledNonCancellableSyncAction)self!)._action(), state: this);
 
            _taskCompletionSource.TrySetResult(null);
        }
        catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource.TrySetException(
                new ExecutionDeclinedException(ex));

            return Task.FromCanceled(executionCancellationToken);
        }
        catch (OperationCanceledException)
        {
            _taskCompletionSource.TrySetCanceled();
        }
        catch (Exception ex)
        {
            _taskCompletionSource.TrySetException(ex);
        }

        return Task.CompletedTask;
    }

    public void Wait() => _taskCompletionSource.Task.GetAwaiter().GetResult();
}
