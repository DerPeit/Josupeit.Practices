# Josupeit.Practices.Jobs.Abstractions

A scheduler runs fire-and-forget work items. A job is something richer: it has an observable lifecycle, an identity, and it may carry a result or report incremental progress. This package defines the contracts for that model.

The typical flow is: create a job through a factory, start it, observe its state via the `Changed` event or by awaiting it directly, and read the result or exception once it completes.

```
dotnet add package Josupeit.Practices.Jobs.Abstractions
```

## Core interface: `IJob`

```csharp
public interface IJob
{
    Guid       Id                      { get; }  // stable identity across the job's lifetime
    bool       IsPending               { get; }  // submitted but not yet picked up by the executor
    bool       IsExecuting             { get; }  // currently running
    bool       IsCompleted             { get; }  // true for canceled, faulted, and successful outcomes
    bool       IsCompletedSuccessfully { get; }
    bool       IsFaulted               { get; }
    bool       IsCanceled              { get; }
    Exception? Exception               { get; }  // set when IsFaulted is true
    event EventHandler Changed;                  // raised on every state transition

    void Start();
    void Cancel();
}
```

## Job variants

Plain `IJob` covers the most common case. When you need more, use one of the typed variants.

| Interface | Additional member |
|---|---|
| `IJobWithResult<TResult>` | `TResult Result` — available once `IsCompletedSuccessfully` is `true` |
| `IJobWithProgress<TProgress>` | `TProgress Progress` — updated via `IProgress<TProgress>` callbacks during execution |
| `IJobWithProgressAndResult<TProgress, TResult>` | Both of the above |

## Creating jobs: `IJobFactory`

`IJobFactory` creates job instances from async delegates. Every overload accepts either a plain `Func<Task>` or a cancellable `Func<CancellationToken, Task>`. The typed variants add `IProgress<TProgress>` and `Task<TResult>` as appropriate.

## Awaiting a job

Jobs are awaitable out of the box. The `GetAwaiter()` extension method and a synchronous `Wait()` counterpart are both provided by this package, so you can integrate jobs naturally into async code.

```csharp
IJobFactory factory = /* TaskPoolJobFactory or SchedulerJobFactory */;

IJob job = factory.Create(async ct => await DoWorkAsync(ct));
job.Start();

await job;     // awaits completion; does not throw if the job was canceled or faulted
job.Wait();    // synchronous equivalent

// Read the outcome.
if (job.IsFaulted)
    Console.WriteLine(job.Exception);
```

For concrete factory implementations, use `Josupeit.Practices.Jobs.TaskPool` or `Josupeit.Practices.Jobs.Scheduler`.
