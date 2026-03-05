using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledCancellableAsyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly Func<CancellationToken, Task<TResult>>? _cancellableAsyncFunction;
    private readonly Task<TResult>? _prematurePromise;
    private readonly CancellationToken? _scheduleCancellationToken;
    private readonly ExecutionContext? _executionContext;
    private readonly TaskCompletionSource<TResult>? _taskCompletionSource;

    private IDisposable? _prematureCancellationRegistration;

    public ScheduledCancellableAsyncFunction(Func<CancellationToken, Task<TResult>> cancellableAsyncFunction, CancellationToken scheduleCancellationToken, ExecutionContext? executionContext)
    {
        if (scheduleCancellationToken.IsCancellationRequested)
        {
            _prematurePromise = Task.FromCanceled<TResult>(scheduleCancellationToken);
            return;
        }

        _cancellableAsyncFunction = cancellableAsyncFunction;
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

    public void Execute(CancellationToken executionCancellationToken) => ExecuteAsync(executionCancellationToken).GetAwaiter().GetResult();

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

        var cancellationTokenSource = _scheduleCancellationToken.Value.CanBeCanceled && executionCancellationToken.CanBeCanceled
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

            Task<TResult> promise = null!;

            ExecutionContext.Run(_executionContext!, _ => promise = _cancellableAsyncFunction!(cancellationToken), state: null);

            switch (promise)
            {
                case { IsCompleted: true, IsCanceled: false, IsFaulted: false } task:
                    _taskCompletionSource!.TrySetResult(task.Result);

                    cancellationTokenSource?.Dispose();

                    return Task.CompletedTask;

                case { IsCanceled: true } when executionCancellationToken.IsCancellationRequested:
                    _taskCompletionSource!.TrySetException(
                        new ExecutionDeclinedException());

                    cancellationTokenSource?.Dispose();

                    return Task.FromCanceled(executionCancellationToken);

                case { IsCanceled: true } when _scheduleCancellationToken.Value.IsCancellationRequested:
                    _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);

                    cancellationTokenSource?.Dispose();

                    return Task.CompletedTask;

                case { IsCanceled: true }:
                    _taskCompletionSource!.TrySetCanceled();

                    cancellationTokenSource?.Dispose();

                    return Task.CompletedTask;

                case { IsFaulted: true, Exception.InnerException: var ex }:
                    _taskCompletionSource!.TrySetException(ex!);

                    cancellationTokenSource?.Dispose();

                    return Task.CompletedTask;

                default:
                    return Awaited(this, cancellationTokenSource, executionCancellationToken, promise);
            }
        }
        catch (OperationCanceledException) when (_scheduleCancellationToken.Value.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetCanceled(_scheduleCancellationToken.Value);

            cancellationTokenSource?.Dispose();
        }
        catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
        {
            _taskCompletionSource!.TrySetException(
                new ExecutionDeclinedException(ex));

            cancellationTokenSource?.Dispose();
            throw;
        }
        catch (OperationCanceledException)
        {
            _taskCompletionSource!.TrySetCanceled();

            cancellationTokenSource?.Dispose();
        }
        catch (Exception ex)
        {
            _taskCompletionSource!.TrySetException(ex);

            cancellationTokenSource?.Dispose();
        }

        return Task.CompletedTask;

        static async Task Awaited(ScheduledCancellableAsyncFunction<TResult> owner, CancellationTokenSource? cancellationTokenSource, CancellationToken executionCancellationToken, Task<TResult> task)
        {
            using (cancellationTokenSource)
            {
                try
                {
                    var result = await task.ConfigureAwait(false);

                    owner._taskCompletionSource!.TrySetResult(result);
                }
                catch (OperationCanceledException ex) when (executionCancellationToken.IsCancellationRequested)
                {
                    owner._taskCompletionSource!.TrySetException(
                        new ExecutionDeclinedException(ex));

                    throw;
                }
                catch (OperationCanceledException) when (owner._scheduleCancellationToken!.Value.IsCancellationRequested)
                {
                    owner._taskCompletionSource!.TrySetCanceled(owner._scheduleCancellationToken.Value);
                }
                catch (OperationCanceledException)
                {
                    owner._taskCompletionSource!.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    owner._taskCompletionSource!.TrySetException(ex);
                }
            }
        }
    }

    public TResult WaitAndUnwrap() => Promise.GetAwaiter().GetResult();
}