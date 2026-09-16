// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

#pragma warning disable CA1849, RS0030 // .Result is guarded by IsCompleted; the access is synchronous by construction.
#pragma warning disable CA2016 // TrySetCanceled() is deliberately parameterless; the cancellation source is ambiguous in catch blocks.

internal sealed class ScheduledNonCancellableAsyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Func<Task<TResult>> _asyncFunction;
    private readonly ExecutionContext _executionContext;

    public ScheduledNonCancellableAsyncFunction(Func<Task<TResult>> asyncFunction, ExecutionContext? executionContext)
    {
        _asyncFunction = asyncFunction;
        _executionContext = executionContext ?? Singletons.DefaultExecutionContext;
    }

    bool IScheduled.IsCancellationRequested => false;

    public Task<TResult> Promise => _taskCompletionSource.Task;

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
            Task<TResult> promise = null!;

            ExecutionContext.Run(_executionContext, _ => promise = _asyncFunction(), state: null);

            switch (promise)
            {
                case { IsCompleted: true, IsCanceled: false, IsFaulted: false } task:
                    _taskCompletionSource.TrySetResult(task.Result);

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

        static async Task Awaited(ScheduledNonCancellableAsyncFunction<TResult> owner, CancellationToken executionCancellationToken, Task<TResult> task)
        {
            try
            {
                var result = await task.ConfigureAwait(false);

                owner._taskCompletionSource.TrySetResult(result);
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

    public TResult WaitAndUnwrap() => _taskCompletionSource.Task.GetAwaiter().GetResult();
}
