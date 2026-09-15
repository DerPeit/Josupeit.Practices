// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

#pragma warning disable CA1068 // CancellationToken groups logically with the delegate it cancels, not the trailing ExecutionContext.
#pragma warning disable CA2016 // TrySetCanceled() is deliberately parameterless; the cancellation source is ambiguous in catch blocks.

internal sealed class ScheduledCancellableSyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly Func<CancellationToken, TResult>? _cancellableFunction;
    private readonly Task<TResult>? _prematurePromise;
    private readonly CancellationToken? _scheduleCancellationToken;
    private readonly ExecutionContext? _executionContext;
    private readonly TaskCompletionSource<TResult>? _taskCompletionSource;

    private IDisposable? _prematureCancellationRegistration;

    public ScheduledCancellableSyncFunction(Func<CancellationToken, TResult> cancellableFunction, CancellationToken scheduleCancellationToken, ExecutionContext? executionContext)
    {
        if (scheduleCancellationToken.IsCancellationRequested)
        {
            _prematurePromise = Task.FromCanceled<TResult>(scheduleCancellationToken);
            return;
        }

        _cancellableFunction = cancellableFunction;
        _scheduleCancellationToken = scheduleCancellationToken;
        _executionContext = executionContext ?? Singletons.DefaultExecutionContext;
        _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!scheduleCancellationToken.CanBeCanceled)
            return;

        // If the cancellation token that was used in the IScheduler.Schedule call signals cancellation
        // before the scheduler picks it up and starts execution, we transition the underlying task
        // to a canceled state immediately. Otherwise, the call-site would have to wait until the
        // scheduler starts executing and likely immediately canceling it.
        _prematureCancellationRegistration = scheduleCancellationToken.Register(() => _taskCompletionSource!.TrySetCanceled(scheduleCancellationToken));
    }

    public bool IsCancellationRequested => _prematurePromise is not null || _scheduleCancellationToken!.Value.IsCancellationRequested;

    public Task<TResult> Promise => _prematurePromise ?? _taskCompletionSource!.Task;

    public void Decline()
    {
        if (_prematurePromise is not null)
            return; // because this instance has already been created with a canceled token.

        _prematureCancellationRegistration?.Dispose();
        _prematureCancellationRegistration = null;

        _taskCompletionSource!.TrySetException(
            new ExecutionDeclinedException());
    }

    public void Execute(CancellationToken executionCancellationToken)
    {
        if (_prematurePromise is not null)
            return; // because this instance has already been created with a canceled token.

        _prematureCancellationRegistration?.Dispose();
        _prematureCancellationRegistration = null;

        if (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetException(
                new ExecutionDeclinedException());

            throw new OperationCanceledException(executionCancellationToken);
        }

        if (_scheduleCancellationToken!.Value.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);

            return;
        }

        using var cancellationTokenSource = _scheduleCancellationToken.Value.CanBeCanceled && executionCancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(_scheduleCancellationToken.Value, executionCancellationToken)
            : null;

        try
        {
            var cancellationToken = (cancellationTokenSource, _scheduleCancellationToken.Value) switch
            {
                (not null, _) => cancellationTokenSource.Token,
                (null, { CanBeCanceled: true }) => _scheduleCancellationToken.Value,
                _ => executionCancellationToken,
            };

            TResult result = default!;

            ExecutionContext.Run(_executionContext!, _ => result = _cancellableFunction!(cancellationToken), state: null);

            _taskCompletionSource!.TrySetResult(result);
        }
        catch (OperationCanceledException) when (_scheduleCancellationToken.Value.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);
        }
        catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetException(
                new ExecutionDeclinedException(ex));

            throw;
        }
        catch (OperationCanceledException)
        {
            _taskCompletionSource!.TrySetCanceled();
        }
        catch (Exception ex)
        {
            _taskCompletionSource!.TrySetException(ex);
        }
    }

    public Task ExecuteAsync(CancellationToken executionCancellationToken)
    {
        if (_prematurePromise is not null)
            return Task.CompletedTask; // because this instance has already been created with a canceled token.

        _prematureCancellationRegistration?.Dispose();
        _prematureCancellationRegistration = null;

        if (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetException(
                new ExecutionDeclinedException());

            return Task.FromCanceled(executionCancellationToken);
        }

        if (_scheduleCancellationToken!.Value.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);

            return Task.CompletedTask;
        }

        using var cancellationTokenSource = _scheduleCancellationToken.Value.CanBeCanceled && executionCancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(_scheduleCancellationToken.Value, executionCancellationToken)
            : null;

        try
        {
            var cancellationToken = (cancellationTokenSource, _scheduleCancellationToken.Value) switch
            {
                (not null, _) => cancellationTokenSource.Token,
                (null, { CanBeCanceled: true }) => _scheduleCancellationToken.Value,
                _ => executionCancellationToken,
            };

            TResult result = default!;

            ExecutionContext.Run(_executionContext!, _ => result = _cancellableFunction!(cancellationToken), state: null);

            _taskCompletionSource!.TrySetResult(result);
        }
        catch (OperationCanceledException) when (_scheduleCancellationToken.Value.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);
        }
        catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetException(
                new ExecutionDeclinedException(ex));

            return Task.FromCanceled(executionCancellationToken);
        }
        catch (OperationCanceledException)
        {
            _taskCompletionSource!.TrySetCanceled();
        }
        catch (Exception ex)
        {
            _taskCompletionSource!.TrySetException(ex);
        }

        return Task.CompletedTask;
    }

    public TResult WaitAndUnwrap() => Promise.GetAwaiter().GetResult();
}
