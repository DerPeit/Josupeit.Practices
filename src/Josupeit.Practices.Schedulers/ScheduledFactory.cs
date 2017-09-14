using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledFactory : IScheduledFactory
{
    public IScheduledAction Create(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        return new ScheduledNonCancellableSyncAction(action);
    }

    public IScheduledAction Create(Action<CancellationToken> cancellableAction, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        return new ScheduledCancellableSyncAction(cancellableAction, scheduleCancellationToken);
    }

    public IScheduledAction Create(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        return new ScheduledNonCancellableAsyncAction(asyncAction);
    }

    public IScheduledAction Create(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        return new ScheduledCancellableAsyncAction(cancellableAsyncAction, scheduleCancellationToken);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        return new ScheduledNonCancellableSyncFunction<TResult>(func);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        return new ScheduledCancellableSyncFunction<TResult>(cancellableFunc, scheduleCancellationToken);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        return new ScheduledNonCancellableAsyncFunction<TResult>(asyncFunc);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken scheduleCancellationToken = default)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        return new ScheduledCancellableAsyncFunction<TResult>(cancellableAsyncFunc, scheduleCancellationToken);
    }
}