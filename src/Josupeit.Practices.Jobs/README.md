# Josupeit.Practices.Jobs.Core

This package contains `JobBase` and the four abstract job classes built on it — `Job`, `JobWithResult<TResult>`, `JobWithProgress<TProgress>`, and `JobWithProgressAndResult<TProgress, TResult>` — that both `TaskPoolJobFactory` and `SchedulerJobFactory` derive their concrete job types from. You would typically reach for this package only when you need to implement a completely custom job type — for instance, one that integrates with a third-party execution engine — while still fitting into the `IJob`-based observable model.

If you just need to run async delegates as trackable jobs, one of the two ready-made factories is almost certainly the better choice.

```
dotnet add package Josupeit.Practices.Jobs.Core
```

All types live in the `Josupeit.Practices.Jobs` namespace, shared by every Jobs package; the package name's `.Core` suffix is not part of it.

## What the base classes provide

`JobBase` supplies everything that is common across all job types:

- A stable `Id` (`Guid.NewGuid()` at construction time)
- The six boolean state properties (`IsPending`, `IsExecuting`, `IsCompleted`, `IsCompletedSuccessfully`, `IsFaulted`, `IsCanceled`)
- The `Exception` property for faulted outcomes
- The `Changed` event and the protected `OnChanged()` hook
- A `CancellationTokenSource` exposed as the protected `CancellationToken` property
- `Cancel()` — requests cancellation of that token on a thread-pool thread, so cancellation callbacks never run inline in the caller
- Guard methods `ThrowIfCannotStart()` and `ThrowIfCannotCancel()` to enforce valid transitions
- The protected transition methods `NotifyPending()`, `NotifyExecuting()`, `NotifyFailed(Exception)`, and `NotifyCanceled()`

`JobBase` itself does not implement `IJob`. The four job classes do: each implements the matching interface, adds `NotifySucceeded()` (with the result, where there is one) and, on the progress variants, the protected `NotifyProgress(TProgress?)`. Derive from one of them, not from `JobBase` directly, and implement `Start()`.

## How to implement a custom job

Validate and mark the job pending synchronously, so that a misuse such as starting twice reaches the caller as an `InvalidOperationException`; then run the work asynchronously and advance the state through the `Notify*` methods. Every path must end in exactly one of `NotifySucceeded`, `NotifyFailed`, or `NotifyCanceled`, otherwise `await job` never completes.

```csharp
public sealed class MyCustomJob : Job
{
    private readonly Func<CancellationToken, Task> _work;

    public MyCustomJob(Func<CancellationToken, Task> work) => _work = work ?? throw new ArgumentNullException(nameof(work));

    public override void Start()
    {
        ThrowIfCannotStart();
        NotifyPending();

        // Fire and forget — every outcome is reported through a Notify* method inside RunAsync.
        // Task.Run leaves the caller's thread; an awaited Task.Yield() would return to the caller's SynchronizationContext.
        _ = Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        NotifyExecuting();

        try
        {
            await _work(CancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
        {
            NotifyCanceled();
            return;
        }
        catch (Exception ex)
        {
            NotifyFailed(ex);
            return;
        }

        NotifySucceeded();
    }
}
```

For a result, derive from `JobWithResult<TResult>` and call `NotifySucceeded(result)`; for progress, derive from `JobWithProgress<TProgress>` and call `NotifyProgress(value)` while running, for example from an `IProgress<TProgress>` you hand to the work.
