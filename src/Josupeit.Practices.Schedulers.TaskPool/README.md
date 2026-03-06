# Josupeit.Practices.Schedulers.TaskPool

Sometimes scheduling just means "run this on the thread pool and let me know when it's done." There is no queue, no ordering requirement, and no need to start or stop anything. `TaskPoolScheduler` is exactly that: the simplest possible `IScheduler` implementation, suitable for fire-and-go workloads where every item can execute independently and concurrently.

Because it extends `Scheduler` from `Josupeit.Practices.Schedulers.Core`, you get the full `IScheduler` API — sync and async, actions and functions, cancellable variants — without any lifecycle ceremony.

```
dotnet add package Josupeit.Practices.Schedulers.TaskPool
```

## How to use

```csharp
IScheduler scheduler = new TaskPoolScheduler();

// Fire a synchronous action. The call returns once the action has completed.
scheduler.Schedule(() => DoWork());

// Fire an asynchronous action and await its completion from the calling thread.
await scheduler.ScheduleAsync(async ct => await DoWorkAsync(ct), cancellationToken);

// Schedule a function and retrieve its return value.
int value = scheduler.Schedule(() => ComputeValue());
Task<int> pending = scheduler.ScheduleAsync(ct => Task.FromResult(ComputeValue()), cancellationToken);
```

Because items are dispatched to the thread pool immediately and independently of one another, there is no guaranteed ordering between them. If your workload requires items to run sequentially — or if you need back-pressure, pausing, or bounded concurrency — use `Josupeit.Practices.Schedulers.Channel` instead.
