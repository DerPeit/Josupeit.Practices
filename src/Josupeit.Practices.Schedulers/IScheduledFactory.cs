using System.Threading.Tasks;
using System.Threading;
using System;

namespace Josupeit.Practices.Schedulers;

internal interface IScheduledFactory
{
    IScheduledAction Create(Action action);

    IScheduledAction Create(Action<CancellationToken> cancellableAction, CancellationToken scheduleCancellationToken = default);

    IScheduledAction Create(Func<Task> asyncAction);

    IScheduledAction Create(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken scheduleCancellationToken = default);

    IScheduledFunction<TResult> Create<TResult>(Func<TResult> func);

    IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken scheduleCancellationToken = default);

    IScheduledFunction<TResult> Create<TResult>(Func<Task<TResult>> asyncFunc);
    
    IScheduledFunction<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken scheduleCancellationToken = default);
}
