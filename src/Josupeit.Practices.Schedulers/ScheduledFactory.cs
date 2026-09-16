// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Josupeit.Practices.Schedulers;

internal sealed class ScheduledFactory : IScheduledFactory
{
    public IScheduledAction Create(Action action, ExecutionContext? executionContext)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new ScheduledNonCancellableSyncAction(action, executionContext);
    }

    public IScheduledAction Create(Action<CancellationToken> cancellableAction, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellableAction);

        return new ScheduledCancellableSyncAction(cancellableAction, scheduleCancellationToken, executionContext);
    }

    public IScheduledAction Create(Func<Task> asyncAction, ExecutionContext? executionContext)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        return new ScheduledNonCancellableAsyncAction(asyncAction, executionContext);
    }

    public IScheduledAction Create(Func<CancellationToken, Task> cancellableAsyncAction, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellableAsyncAction);

        return new ScheduledCancellableAsyncAction(cancellableAsyncAction, scheduleCancellationToken, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<TResult> func, ExecutionContext? executionContext)
    {
        ArgumentNullException.ThrowIfNull(func);

        return new ScheduledNonCancellableSyncFunction<TResult>(func, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, TResult> cancellableFunc, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellableFunc);

        return new ScheduledCancellableSyncFunction<TResult>(cancellableFunc, scheduleCancellationToken, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<Task<TResult>> asyncFunc, ExecutionContext? executionContext)
    {
        ArgumentNullException.ThrowIfNull(asyncFunc);

        return new ScheduledNonCancellableAsyncFunction<TResult>(asyncFunc, executionContext);
    }

    public IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, ExecutionContext? executionContext, CancellationToken scheduleCancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellableAsyncFunc);

        return new ScheduledCancellableAsyncFunction<TResult>(cancellableAsyncFunc, scheduleCancellationToken, executionContext);
    }
}
