using System.Threading.Tasks;
using System.Threading;
using System;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledFactory
{
    IScheduledAction Create(
        Action action,
        ExecutionContext? executionContext);

    IScheduledAction Create(
        Action<CancellationToken> cancellableAction,
        ExecutionContext? executionContext,
        CancellationToken scheduleCancellationToken = default);

    IScheduledAction Create(
        Func<Task> asyncAction,
        ExecutionContext? executionContext);

    IScheduledAction Create(
        Func<CancellationToken, Task> cancellableAsyncAction,
        ExecutionContext? executionContext,
        CancellationToken scheduleCancellationToken = default);

    IScheduledFunction<TResult> Create<TResult>(
        Func<TResult> func,
        ExecutionContext? executionContext);

    IScheduledFunction<TResult> Create<TResult>(
        Func<CancellationToken, TResult> cancellableFunc,
        ExecutionContext? executionContext,
        CancellationToken scheduleCancellationToken = default);

    IScheduledFunction<TResult> Create<TResult>(
        Func<Task<TResult>> asyncFunc,
        ExecutionContext? executionContext);

    IScheduledFunction<TResult> Create<TResult>(
        Func<CancellationToken, Task<TResult>> cancellableAsyncFunc,
        ExecutionContext? executionContext,
        CancellationToken scheduleCancellationToken = default);
}
