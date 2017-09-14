using System;
using System.Threading;
using System.Threading.Tasks;
using Josupeit.Practices.Disposables;
using Josupeit.Practices.Services;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This class is a base class and represents a scheduler that executes particular operations eventually.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class Scheduler : IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public Scheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal Scheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

/// <summary>
/// This class is a base class and represents a scheduler that executes particular operations eventually and which owns resources that needs to be released.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class DisposableScheduler : ImplicitDisposable, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public DisposableScheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal DisposableScheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

/// <summary>
/// This class is a base class and represents a scheduler that executes particular operations eventually and which owns resources that needs to be released asynchronously.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncDisposableScheduler : ImplicitAsyncDisposable, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncDisposableScheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal AsyncDisposableScheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

#endif
/// <summary>
/// This class is a base class and represents an async scheduler that executes particular operations eventually.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncScheduler : IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncScheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal AsyncScheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

/// <summary>
/// This class is a base class and represents an async scheduler that executes particular operations eventually and which owns resources that needs to be released.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class DisposableAsyncScheduler : ImplicitDisposable, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public DisposableAsyncScheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal DisposableAsyncScheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

/// <summary>
/// This class is a base class and represents an async scheduler that executes particular operations eventually and which owns resources that needs to be released asynchronously.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncDisposableAsyncScheduler : ImplicitAsyncDisposable, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncDisposableAsyncScheduler()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal AsyncDisposableAsyncScheduler(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

#endif
/// <summary>
/// This class is a base class and represents a scheduler service that executes particular operations eventually.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class SchedulerService : Service, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public SchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal SchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

/// <summary>
/// This class is a base class and represents a scheduler service that executes particular operations eventually and which owns resources that needs to be released.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class DisposableSchedulerService : DisposableService, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public DisposableSchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal DisposableSchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

/// <summary>
/// This class is a base class and represents a scheduler service that executes particular operations eventually and which owns resources that needs to be released asynchronously.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncDisposableSchedulerService : AsyncDisposableService, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncDisposableSchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new SyncEnqueueBehavior(Enqueue);
    }

    internal AsyncDisposableSchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract void Enqueue(IScheduled scheduled);
}

#endif
/// <summary>
/// This class is a base class and represents an async scheduler service that executes particular operations eventually.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncSchedulerService : AsyncService, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncSchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal AsyncSchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

/// <summary>
/// This class is a base class and represents an async scheduler service that executes particular operations eventually and which owns resources that needs to be released.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class DisposableAsyncSchedulerService : DisposableAsyncService, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public DisposableAsyncSchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal DisposableAsyncSchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER

/// <summary>
/// This class is a base class and represents an async scheduler service that executes particular operations eventually and which owns resources that needs to be released asynchronously.
/// </summary>
/// <seealso cref="IScheduler" />
public abstract class AsyncDisposableAsyncSchedulerService : AsyncDisposableAsyncService, IScheduler
{
    private readonly IScheduledFactory _scheduledFactory;
    private readonly IEnqueueBehavior _enqueueBehavior;

    public AsyncDisposableAsyncSchedulerService()
    {
        _scheduledFactory = new ScheduledFactory();
        _enqueueBehavior = new AsyncEnqueueBehavior(EnqueueAsync);
    }

    internal AsyncDisposableAsyncSchedulerService(IScheduledFactory scheduledFactory, IEnqueueBehavior enqueueBehavior)
    {
        _scheduledFactory = scheduledFactory;
        _enqueueBehavior = enqueueBehavior;
    }

    /// <inheritdoc />
    public void Schedule(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction) => Schedule(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<Task> asyncAction)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        _enqueueBehavior.Enqueue(scheduled);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction) => Schedule(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        scheduled.Wait();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<TResult> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc) => Schedule(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        _enqueueBehavior.Enqueue(scheduled);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => Schedule(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        _enqueueBehavior.Enqueue(scheduled, cancellationToken);

        return scheduled.WaitAndUnwrap();
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action action) => ScheduleAsync(action, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var scheduled = _scheduledFactory.Create(action);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action<CancellationToken> cancellableAction) => ScheduleAsync(cancellableAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken)
    {
        if (cancellableAction == null)
            throw new ArgumentNullException(nameof(cancellableAction));

        var scheduled = _scheduledFactory.Create(cancellableAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<Task> asyncAction) => ScheduleAsync(asyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken)
    {
        if (asyncAction == null)
            throw new ArgumentNullException(nameof(asyncAction));

        var scheduled = _scheduledFactory.Create(asyncAction);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction) => ScheduleAsync(cancellableAsyncAction, CancellationToken.None);

    /// <inheritdoc />
    public async Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken)
    {
        if (cancellableAsyncAction == null)
            throw new ArgumentNullException(nameof(cancellableAsyncAction));

        var scheduled = _scheduledFactory.Create(cancellableAsyncAction, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<TResult> func) => ScheduleAsync(func, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var scheduled = _scheduledFactory.Create(func);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc) => ScheduleAsync(cancellableFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken)
    {
        if (cancellableFunc == null)
            throw new ArgumentNullException(nameof(cancellableFunc));

        var scheduled = _scheduledFactory.Create(cancellableFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc) => ScheduleAsync(asyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken)
    {
        if (asyncFunc == null)
            throw new ArgumentNullException(nameof(asyncFunc));

        var scheduled = _scheduledFactory.Create(asyncFunc);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc) => ScheduleAsync(cancellableAsyncFunc, CancellationToken.None);

    /// <inheritdoc />
    public async Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken)
    {
        if (cancellableAsyncFunc == null)
            throw new ArgumentNullException(nameof(cancellableAsyncFunc));

        var scheduled = _scheduledFactory.Create(cancellableAsyncFunc, cancellationToken);

        await _enqueueBehavior.EnqueueAsync(scheduled, cancellationToken).ConfigureAwait(false);

        return await scheduled.Promise.ConfigureAwait(false);
    }

    protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken);
    }

#endif
