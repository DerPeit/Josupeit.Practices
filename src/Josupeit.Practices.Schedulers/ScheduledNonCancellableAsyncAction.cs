// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

#pragma warning disable CA2016 // TrySetCanceled() is deliberately parameterless; the cancellation source is ambiguous in catch blocks.

internal sealed class ScheduledNonCancellableAsyncAction : IScheduledAction
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Func<Task> _asyncAction;
    private readonly ExecutionContext _executionContext;

    public ScheduledNonCancellableAsyncAction(Func<Task> asyncAction, ExecutionContext? executionContext)
    {
        _asyncAction = asyncAction;
        _executionContext = executionContext ?? Singletons.DefaultExecutionContext;
    }

    bool IScheduled.IsCancellationRequested => false;

    public Task Promise => _taskCompletionSource.Task;

    public void Decline()
    {
        _taskCompletionSource.TrySetException(
            new ExecutionDeclinedException());
    }

    public void Execute(CancellationToken executionCancellationToken) => ExecuteAsync(executionCancellationToken).GetAwaiter().GetResult();

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
            Task promise = null!;

            ExecutionContext.Run(_executionContext, _ => promise = _asyncAction(), state: null);

            switch (promise)
            {
                case { IsCompleted: true, IsCanceled: false, IsFaulted: false }:
                    _taskCompletionSource.TrySetResult(null);

                    return Task.CompletedTask;

                case { IsCanceled: true } when executionCancellationToken.IsCancellationRequested:
                    _taskCompletionSource.TrySetException(
                        new ExecutionDeclinedException());

                    return Task.FromCanceled(executionCancellationToken);

                case { IsCanceled: true }:
                    _taskCompletionSource.TrySetCanceled();

                    return Task.CompletedTask;

                case { IsFaulted: true, Exception.InnerException: var ex }:
                    _taskCompletionSource.TrySetException(ex!);

                    return Task.CompletedTask;

                default:
                    return Awaited(this, executionCancellationToken, promise);
            }
        }
        catch (Exception ex)
        {
            _taskCompletionSource.TrySetException(ex);
        }

        return Task.CompletedTask;

        static async Task Awaited(ScheduledNonCancellableAsyncAction owner, CancellationToken executionCancellationToken, Task task)
        {
            try
            {
                await task.ConfigureAwait(false);

                owner._taskCompletionSource.TrySetResult(null);
            }
            catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
            {
                owner._taskCompletionSource.TrySetException(
                    new ExecutionDeclinedException(ex));

                throw;
            }
            catch (OperationCanceledException)
            {
                owner._taskCompletionSource.TrySetCanceled();
            }
            catch (Exception ex)
            {
                owner._taskCompletionSource.TrySetException(ex);
            }
        }
    }

    public void Wait() => _taskCompletionSource.Task.GetAwaiter().GetResult();
}
