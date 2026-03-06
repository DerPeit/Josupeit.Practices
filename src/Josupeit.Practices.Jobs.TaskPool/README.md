# Josupeit.Practices.Jobs.TaskPool

The simplest way to turn an async delegate into a trackable, observable job. `TaskPoolJobFactory` creates jobs that start executing on the thread pool the moment `Start()` is called. There is no queue, no ordering guarantee, and no scheduler lifecycle to manage. It is the right default when all you need is the ability to track whether a background operation is still running, what it returned, or whether it failed.

```
dotnet add package Josupeit.Practices.Jobs.TaskPool
```

## How to use

### Simple job

```csharp
IJobFactory factory = new TaskPoolJobFactory();

IJob job = factory.Create(async ct => await DoWorkAsync(ct));
job.Start();

// Await completion. Does not throw — check IsFaulted / IsCanceled afterward if needed.
await job;
```

### Job with a result

```csharp
IJobWithResult<int> job = factory.Create(async ct =>
{
    await Task.Delay(100, ct);
    return 42;
});
job.Start();
await job;

if (job.IsCompletedSuccessfully)
    Console.WriteLine(job.Result); // 42
```

### Job with progress reporting

Progress is delivered to the `Progress` property and the `Changed` event is raised on every update, making it easy to drive a progress bar or status label.

```csharp
IJobWithProgress<int> job = factory.Create<int>(async (progress, ct) =>
{
    for (int i = 0; i <= 100; i++)
    {
        await Task.Delay(10, ct);
        progress.Report(i);
    }
});

job.Changed += (_, _) => Console.WriteLine($"{job.Progress}%");
job.Start();
await job;
```

### Job with both progress and a result

```csharp
IJobWithProgressAndResult<int, string> job = factory.Create<int, string>(async (progress, ct) =>
{
    progress.Report(50);
    await Task.Delay(50, ct);
    progress.Report(100);
    return "done";
});
job.Start();
await job;
```

When you need jobs to run in a specific order or with bounded concurrency, use `Josupeit.Practices.Jobs.Scheduler` together with a `ChannelScheduler` instead.
