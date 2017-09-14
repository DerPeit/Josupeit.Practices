using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledNonCancellableAsyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Func<Task<TResult>> _asyncFunction;

    public ScheduledNonCancellableAsyncFunction(Func<Task<TResult>> asyncFunction)
    {
        _asyncFunction = asyncFunction;
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
            switch (_asyncFunction())
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

                case { } task:
                    return Awaited(this, executionCancellationToken, task);
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