# Josupeit.Practices.Schedulers.Abstractions

Different parts of an application have different scheduling needs. One component may need to fire work on the thread pool without any ordering guarantee; another may need a bounded queue with back-pressure and pause support. When you program against a concrete scheduler type directly, switching strategies later requires touching every call site.

`IScheduler` gives you a single interface for scheduling work regardless of how or where that work is actually executed. It is deliberately broad — covering actions, functions, sync lambdas, async lambdas, and cancellable variants of each — so that call sites never need to adapt their code to fit the scheduler.

```
dotnet add package Josupeit.Practices.Schedulers.Abstractions
```

## Method families

`IScheduler` organizes its methods into three families. Each family supports both actions (`void`/`Task` return) and functions (`TResult`/`Task<TResult>` return), and both non-cancellable and cancellable variants.

| Family | Behavior |
|---|---|
| `Schedule(…)` | Enqueues the work item and blocks the calling thread until it has finished |
| `ScheduleAsync(…)` | Enqueues the work item and returns a `Task` that completes when the item finishes |
| `UnsafeSchedule(…)` / `UnsafeScheduleAsync(…)` | Same as above, but does not capture the ambient `ExecutionContext` — useful in tight inner loops where the context-capture allocation matters |

When a scheduler is stopped, submitting work through any of these methods throws `ExecutionDeclinedException`.

## When to use this package directly

Reference `Josupeit.Practices.Schedulers.Abstractions` in any component that schedules work but should not be tied to a specific scheduler implementation. For ready-made implementations, see:

- `Josupeit.Practices.Schedulers.TaskPool` — immediate thread pool dispatch, no lifecycle
- `Josupeit.Practices.Schedulers.Channel` — FIFO queue with configurable parallelism and pause support
- `Josupeit.Practices.Schedulers.Core` — abstract base classes for writing your own implementation
