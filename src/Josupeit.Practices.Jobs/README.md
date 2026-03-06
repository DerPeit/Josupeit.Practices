# Josupeit.Practices.Jobs.Core

This package contains `JobBase`, the abstract base class that both `TaskPoolJobFactory` and `SchedulerJobFactory` build their concrete job types on. You would typically reach for this package only when you need to implement a completely custom job type — for instance, one that integrates with a third-party execution engine — while still fitting into the `IJob`-based observable model.

If you just need to run async delegates as trackable jobs, one of the two ready-made factories is almost certainly the better choice.

```
dotnet add package Josupeit.Practices.Jobs.Core
```

## What `JobBase` provides

`JobBase` supplies everything that is common across all job types:

- A stable `Id` (`Guid.NewGuid()` at construction time)
- The five boolean state properties (`IsPending`, `IsExecuting`, `IsCompleted`, `IsCompletedSuccessfully`, `IsFaulted`, `IsCanceled`)
- The `Exception` property for faulted outcomes
- The `Changed` event and the protected `OnChanged()` hook
- A `CancellationTokenSource` exposed as `CancellationToken` to subclasses
- `Cancel()` — cancels the token on a thread-pool thread to avoid deadlocks
- Guard methods `ThrowIfCannotStart()` and `ThrowIfCannotCancel()` to enforce valid transitions

## How to implement a custom job

Extend `JobBase`, implement `Start()`, and use the protected `Notify*` methods to advance state and fire `Changed`.

```csharp
public sealed class MyCustomJob : JobBase, IJob
{
    private readonly Func<CancellationToken, Task> _work;

    public MyCustomJob(Func<CancellationToken, Task> work) => _work = work;

    public override void Start()
    {
        ThrowIfCannotStart();
        NotifyPending();

        // Fire and forget — state transitions happen inside ExecuteCoreAsync.
        _ = ExecuteCoreAsync();
    }

    private async Task ExecuteCoreAsync()
    {
        NotifyExecuting();
        try
        {
            await _work(CancellationToken);

            IsCompletedSuccessfully = true;
            IsCompleted = true;
            IsExecuting = false;
            OnChanged();
        }
        catch (OperationCanceledException)
        {
            NotifyCanceled();
        }
        catch (Exception ex)
        {
            NotifyFailed(ex);
        }
    }
}
```
