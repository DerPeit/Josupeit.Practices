using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledNonCancellableSyncFunction<TResult> : IScheduledFunction<TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Func<TResult> _function;

    public ScheduledNonCancellableSyncFunction(Func<TResult> function)
    {
        _function = function;
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
            var result = _function();

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
            var result = _function();

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