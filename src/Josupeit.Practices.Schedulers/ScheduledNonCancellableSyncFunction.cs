// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

#pragma warning disable CA2016 // TrySetCanceled() is deliberately parameterless; the cancellation source is ambiguous in catch blocks.

internal sealed class ScheduledNonCancellableSyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Func<TResult> _function;
    private readonly ExecutionContext _executionContext;

    public ScheduledNonCancellableSyncFunction(Func<TResult> function, ExecutionContext? executionContext)
    {
        _function = function;
        _executionContext = executionContext ?? Singletons.DefaultExecutionContext;
    }

    bool IScheduled.IsCancellationRequested => false;

    public Task<TResult> Promise => _taskCompletionSource.Task;

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
            TResult result = default!;

            ExecutionContext.Run(_executionContext, _ => result = _function(), state: null);

            _taskCompletionSource.TrySetResult(result);
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
            TResult result = default!;

            ExecutionContext.Run(_executionContext, _ => result = _function(), state: null);

            _taskCompletionSource.TrySetResult(result);
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

    public TResult WaitAndUnwrap() => _taskCompletionSource.Task.GetAwaiter().GetResult();
}
