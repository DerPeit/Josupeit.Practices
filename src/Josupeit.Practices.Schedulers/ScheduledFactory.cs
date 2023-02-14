using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledFactory : IScheduledFactory
{
    public IScheduledAction Create(Action action, ExecutionContext? executionContext)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        return new ScheduledNonCancellableSyncAction(action, executionContext);
    }

    public IScheduledAction Create(Action<CancellationToken> cancellableAction, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        return new ScheduledCancellableSyncAction(cancellableAction, scheduleCancellationToken, executionContext);
    }

    public IScheduledAction Create(Func<Task> asyncAction, ExecutionContext? executionContext)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        return new ScheduledNonCancellableAsyncAction(asyncAction, executionContext);
    }

    public IScheduledAction Create(Func<CancellationToken, Task> cancellableAsyncAction, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        return new ScheduledCancellableAsyncAction(cancellableAsyncAction, scheduleCancellationToken, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<TResult> func, ExecutionContext? executionContext)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        return new ScheduledNonCancellableSyncFunction<TResult>(func, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, TResult> cancellableFunc, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        return new ScheduledCancellableSyncFunction<TResult>(cancellableFunc, scheduleCancellationToken, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<Task<TResult>> asyncFunc, ExecutionContext? executionContext)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        return new ScheduledNonCancellableAsyncFunction<TResult>(asyncFunc, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        return new ScheduledCancellableAsyncFunction<TResult>(cancellableAsyncFunc, scheduleCancellationToken, executionContext);
    }
}