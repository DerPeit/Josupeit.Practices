# Josupeit.Practices.Schedulers.Core

When neither the `TaskPoolScheduler` nor the `ChannelScheduler` fits your needs, this package gives you the foundation to build your own `IScheduler` implementation. The heavy lifting — translating the 56-member `IScheduler` surface into a single `Enqueue` call, capturing the execution context, bridging sync and async, linking cancellation tokens, and routing results and exceptions back to the caller — is done for you. You only decide what to do with the work item once it arrives.

```
dotnet add package Josupeit.Practices.Schedulers.Core
```

All types live in the `Josupeit.Practices.Schedulers` namespace, shared by every Schedulers package; the package name's `.Core` suffix is not part of it.

## The work item

Your override receives an `IScheduled`. It wraps the caller's delegate together with the captured execution context, the caller's cancellation token, and a promise the caller is waiting on. Four things you can do with it:

- `IsCancellationRequested` — the caller canceled after the item reached you. Skip it; its caller is already being released.
- `Execute(token)` — runs the delegate now, on this thread. Blocks on asynchronous delegates.
- `ExecuteAsync(token)` — runs the delegate now; the returned task completes when the delegate has.
- `Decline()` — refuses the item. The caller gets `ExecutionDeclinedException`.

Every accepted item must end in exactly one of `Execute`, `ExecuteAsync`, or `Decline` — otherwise its caller waits forever. The delegate's own exceptions never surface from `Execute`; they travel to the caller unwrapped. The only exception `Execute` throws on its own account is `OperationCanceledException`, when the token _you_ passed is canceled — the caller then receives `ExecutionDeclinedException`.

## Choosing a base class

Twelve classes cover three independent choices; read the name from the end:

- **Lifecycle:** `...Scheduler` has none. `...SchedulerService` extends the `Josupeit.Practices.Services` base classes and gains `Start`/`Stop`, `State`, and the `On*` hooks.
- **Enqueue style:** `Scheduler` and `SchedulerService` give you `protected abstract void Enqueue(IScheduled scheduled)`. `AsyncScheduler` and `AsyncSchedulerService` give you `protected abstract Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)`. The service variants couple this to the lifecycle style: `SchedulerService` extends `Service`, `AsyncSchedulerService` extends `AsyncService`.
- **Disposal prefix:** none, `Disposable` (`IDisposable`), or `AsyncDisposable` (`IDisposable` and `IAsyncDisposable`; netstandard2.1 and net8.0 only).

The base classes do not consult the lifecycle state before calling your override — `Schedule` on a service that was never started still reaches `Enqueue`. Guard it yourself, and decline whatever is still queued when you stop.

### `Scheduler` — no lifecycle

For schedulers that only forward or dispatch. An `IScheduled` cannot be handed to another `IScheduler` directly, but its `ExecuteAsync` is a `Func<CancellationToken, Task>` — schedule that. If the inner scheduler refuses, decline so the outer caller does not wait forever.

```csharp
public sealed class CountingScheduler : Scheduler
{
    private readonly IScheduler _inner;
    private int _submitted;

    public CountingScheduler(IScheduler inner) => _inner = inner;

    public int Submitted => _submitted;

    protected override void Enqueue(IScheduled scheduled)
    {
        Interlocked.Increment(ref _submitted);

        _ = ForwardAsync(scheduled);
    }

    private async Task ForwardAsync(IScheduled scheduled)
    {
        try
        {
            await _inner.UnsafeScheduleAsync(scheduled.ExecuteAsync).ConfigureAwait(false);
        }
        catch (Exception)
        {
            scheduled.Decline();
        }
    }
}

IScheduler scheduler = new CountingScheduler(new TaskPoolScheduler());

scheduler.Schedule(() => DoWork());
int result = scheduler.Schedule(() => ComputeValue());
await scheduler.ScheduleAsync(ct => DoWorkAsync(ct), cancellationToken);
```

### `SchedulerService` — synchronous lifecycle, synchronous enqueue

For a dedicated consumer thread. Submitting while not running throws; stopping declines whatever is still queued.

```csharp
public sealed class SingleThreadScheduler : SchedulerService
{
    private BlockingCollection<IScheduled>? _queue;
    private CancellationTokenSource? _stopping;
    private Thread? _consumer;

    protected override void OnStart()
    {
        _queue = new BlockingCollection<IScheduled>();
        _stopping = new CancellationTokenSource();
        _consumer = new Thread(Consume) { IsBackground = true };

        _consumer.Start();
    }

    protected override void OnStop()
    {
        _stopping!.Cancel();
        _queue!.CompleteAdding();
        _consumer!.Join();

        foreach (var pending in _queue.GetConsumingEnumerable())
            pending.Decline();

        _stopping.Dispose();
        _queue.Dispose();
    }

    protected override void Enqueue(IScheduled scheduled)
    {
        ThrowIfNotRunning();

        _queue!.Add(scheduled);
    }

    private void Consume()
    {
        foreach (var scheduled in _queue!.GetConsumingEnumerable())
        {
            if (_stopping!.IsCancellationRequested)
            {
                scheduled.Decline();
                continue;
            }

            if (scheduled.IsCancellationRequested)
                continue;

            try
            {
                scheduled.Execute(_stopping.Token);
            }
            catch (OperationCanceledException)
            {
                // Stop was requested while this item ran; its caller received ExecutionDeclinedException.
            }
        }
    }
}
```

### `AsyncSchedulerService` — asynchronous lifecycle, asynchronous enqueue

For an enqueue that may itself wait — here for a bounded channel to have room. The writer is completed on stop; an item arriving after that is declined instead of failing with `ChannelClosedException`.

```csharp
public sealed class ChannelQueueScheduler : AsyncSchedulerService
{
    private Channel<IScheduled>? _channel;
    private CancellationTokenSource? _stopping;
    private Task? _consumer;

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        _channel = Channel.CreateBounded<IScheduled>(capacity: 100);
        _stopping = new CancellationTokenSource();
        _consumer = ConsumeAsync(_channel.Reader, _stopping.Token);

        return Task.CompletedTask;
    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        _stopping!.Cancel();
        _channel!.Writer.Complete();

        await _consumer!.ConfigureAwait(false);

        _stopping.Dispose();
    }

    protected override async Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            scheduled.Decline();
            return;
        }

        try
        {
            await _channel.Writer.WriteAsync(scheduled, cancellationToken).ConfigureAwait(false);
        }
        catch (ChannelClosedException)
        {
            scheduled.Decline();
        }
    }

    private static async Task ConsumeAsync(ChannelReader<IScheduled> reader, CancellationToken stopping)
    {
        await foreach (var scheduled in reader.ReadAllAsync().ConfigureAwait(false))
        {
            if (stopping.IsCancellationRequested)
            {
                scheduled.Decline();
                continue;
            }

            if (scheduled.IsCancellationRequested)
                continue;

            try
            {
                await scheduled.ExecuteAsync(stopping).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Stop was requested while this item ran; its caller received ExecutionDeclinedException.
            }
        }
    }
}
```

Use `Execute` in a synchronous consumer and `await ExecuteAsync` in an asynchronous one — `Execute` blocks the thread until an asynchronous delegate has completed. Pass your own stop token rather than `CancellationToken.None`, so a stop can decline items that are already running.
