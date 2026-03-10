# Josupeit.Practices.Schedulers.Channel

When you need more control over how work is executed — ordering, bounded concurrency, back-pressure, or the ability to pause and resume — `ChannelScheduler` is the right tool. It backs the `IScheduler` interface with `System.Threading.Channels`, processing items in FIFO order across a configurable number of concurrent consumers.

Because it extends `AsyncSchedulerService`, it is a full-featured background service as well as a scheduler. It must be started before you submit work and stopped when you are done. Stopping is always graceful: pending items are declined, and running items are given time to finish before they are canceled.

```
dotnet add package Josupeit.Practices.Schedulers.Channel
```

## How to use

```csharp
var options = new ChannelSchedulerOptions
{
    // Run items one at a time for strict sequential ordering.
    DegreeOfParallelism = 1,

    // Give running items up to five seconds to finish when the scheduler is stopped.
    GracefulStopDelay = TimeSpan.FromSeconds(5),
};

var scheduler = new ChannelScheduler(options);
await scheduler.StartAsync();

// Both sync and async work items can be submitted.
scheduler.Schedule(() => ProcessItem());
await scheduler.ScheduleAsync(async ct => await ProcessItemAsync(ct), cancellationToken);

// Pause processing without dropping items already in the queue.
await scheduler.PauseAsync();
await scheduler.ResumeAsync();

// Stop gracefully — pending items are declined and running items are given GracefulStopDelay to finish.
await scheduler.StopAsync();
```

## Options reference

| Property | Default | Description |
|---|---|---|
| `DegreeOfParallelism` | `Environment.ProcessorCount` | Number of work items that may run concurrently. Set to `1` for strict sequential execution. |
| `GracefulStopDelay` | `TimeSpan.FromSeconds(3)` | How long to wait for currently executing items when `Stop` is called. Set to `TimeSpan.Zero` to cancel running items immediately, or to a negative value to wait indefinitely. |
