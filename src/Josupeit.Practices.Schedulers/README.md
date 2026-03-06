# Josupeit.Practices.Schedulers.Core

When neither the `TaskPoolScheduler` nor the `ChannelScheduler` fits your needs, this package gives you the foundation to build your own `IScheduler` implementation. The heavy lifting — translating the rich `IScheduler` API surface into a single `Enqueue` call, managing sync/async bridging, and handling cancellation — is done for you. You only need to decide what to do with the work item once it arrives.

```
dotnet add package Josupeit.Practices.Schedulers.Core
```

## Choosing a base class

Two base classes are available, differing in whether the scheduler itself has a lifecycle.

### `Scheduler` — stateless, always-available

Use `Scheduler` when your implementation does not need to be started or stopped. You override a single method and the base class handles everything else.

```csharp
public sealed class LoggingScheduler : Scheduler
{
    private readonly ILogger _logger;
    private readonly IScheduler _inner;

    public LoggingScheduler(ILogger logger, IScheduler inner)
    {
        _logger = logger;
        _inner = inner;
    }

    protected override void Enqueue(IScheduled scheduled)
    {
        _logger.LogInformation("Scheduling item {Id}", scheduled.GetHashCode());
        _ = scheduled.ExecuteAsync(CancellationToken.None);
    }
}

// No Start/Stop — just use it.
IScheduler scheduler = new LoggingScheduler(logger, new TaskPoolScheduler());
scheduler.Schedule(() => DoWork());
int result = scheduler.Schedule(() => ComputeValue());
await scheduler.ScheduleAsync(async ct => await DoWorkAsync(ct), cancellationToken);
```

### `AsyncSchedulerService` — lifecycle-aware

Use `AsyncSchedulerService` when the scheduler manages background consumers that must be explicitly started and stopped. It extends `AsyncService`, so it inherits the full service lifecycle — including optional pause support — and additionally implements `IScheduler` through an `EnqueueAsync` override.

```csharp
public sealed class MyQueueScheduler : AsyncSchedulerService
{
    private Channel<IScheduled>? _channel;

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        _channel = Channel.CreateBounded<IScheduled>(capacity: 100);
        _ = ConsumeAsync(_channel.Reader);
        return Task.CompletedTask;
    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        _channel!.Writer.Complete();
        await _channel.Reader.Completion;
    }

    protected override Task EnqueueAsync(IScheduled scheduled, CancellationToken cancellationToken)
    {
        return _channel!.Writer.WriteAsync(scheduled, cancellationToken).AsTask();
    }
}
```

`SchedulerService` (sync) follows the same pattern but extends `Service` instead. Use it when the consumers themselves are synchronous.
