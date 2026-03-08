# Josupeit.Practices.Jobs.Scheduler

`TaskPoolJobFactory` is convenient, but it offers no control over how many jobs run at the same time or in what order. When those guarantees matter — for example, when jobs modify shared state, process a queue of user-submitted tasks one at a time, or must be paused while the application is busy — you need to route them through a scheduler.

`SchedulerJobFactory` bridges the job model and the scheduler model: it creates `IJob` instances that, when started, submit their work to a given `IScheduler`. The scheduler's own concurrency and ordering rules apply automatically. Any `IScheduler` implementation works, but `ChannelScheduler` is the natural partner because it supports backpressure, configurable parallelism, and pausing.

```
dotnet add package Josupeit.Practices.Jobs.Scheduler
```

## How to use

### Sequential execution

```csharp
// DegreeOfParallelism = 1 means jobs run strictly one at a time, in the order they were started.
var scheduler = new ChannelScheduler(new ChannelSchedulerOptions { DegreeOfParallelism = 1 });
await scheduler.StartAsync();

IJobFactory factory = new SchedulerJobFactory(scheduler);

IJob step1 = factory.Create(async ct => await StepOneAsync(ct));
IJob step2 = factory.Create(async ct => await StepTwoAsync(ct));

step1.Start();
step2.Start(); // queued behind step1

await step1;
await step2;

await scheduler.StopAsync();
```

### Bounded parallelism

```csharp
// Allow up to four jobs to run concurrently.
var scheduler = new ChannelScheduler(new ChannelSchedulerOptions { DegreeOfParallelism = 4 });
await scheduler.StartAsync();

IJobFactory factory = new SchedulerJobFactory(scheduler);

var jobs = Enumerable
    .Range(0, 20)
    .Select(i => factory.Create(async ct => await ProcessAsync(i, ct)))
    .ToList();

foreach (var job in jobs)
    job.Start();

foreach (var job in jobs)
    await job;

await scheduler.StopAsync();
```

### Pausing

Because `ChannelScheduler` implements `IAsyncService`, you can pause the entire queue. Jobs already executing will continue until they complete; jobs waiting in the queue will not be picked up until the scheduler is resumed.

```csharp
await scheduler.PauseAsync();
// ... no new jobs start ...
await scheduler.ResumeAsync();
```
