# Practices & Patterns

A collection of small, focused .NET libraries for patterns that appear repeatedly in production applications — commands, background services, schedulers, observable jobs, and the dispose pattern — each packaged separately so you only take what you need.

All packages target **netstandard2.0** as a minimum. Where a feature requires it (e.g. `IAsyncDisposable`, `System.Threading.Channels`), an additional target is included. No package requires a specific application framework.

## Packages

### Specifications

Encapsulate business rules as named, composable objects.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Specifications`](src/Josupeit.Practices.Specifications/README.md) | `Specification<TCandidate>` base class with `&&`, `||`, `^`, and `!` operator support and sync/async bridging |

### Commands

CQRS-style command and query abstractions with sync/async bridging and recursion detection.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Commands.Abstractions`](src/Josupeit.Practices.Commands.Abstractions/README.md) | `ICommand`, `ICommand<TInput>`, `ICommandWithOutput<TOutput>`, `IQuery` interface family |
| [`Josupeit.Practices.Commands`](src/Josupeit.Practices.Commands/README.md) | `Command`, `CommandWithOutput`, `Query` abstract base classes |

### Services

Lifecycle management for background services — start, stop, pause, resume.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Services.Abstractions`](src/Josupeit.Practices.Services.Abstractions/README.md) | `IService` and `IAsyncService` interfaces with `ServiceState` enum |
| [`Josupeit.Practices.Services`](src/Josupeit.Practices.Services/README.md) | `Service`, `AsyncService`, and disposable variants — six base classes in total |

### Schedulers

Route work items through a controlled execution environment.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Schedulers.Abstractions`](src/Josupeit.Practices.Schedulers.Abstractions/README.md) | `IScheduler` interface — sync, async, and unsafe variants for actions and functions |
| [`Josupeit.Practices.Schedulers.Core`](src/Josupeit.Practices.Schedulers/README.md) | `Scheduler` and `AsyncSchedulerService` base classes for custom implementations |
| [`Josupeit.Practices.Schedulers.TaskPool`](src/Josupeit.Practices.Schedulers.TaskPool/README.md) | `TaskPoolScheduler` — immediate thread pool dispatch, no lifecycle |
| [`Josupeit.Practices.Schedulers.Channel`](src/Josupeit.Practices.Schedulers.Channel/README.md) | `ChannelScheduler` — FIFO queue, configurable parallelism, pause support |

### Jobs

Observable background job execution with lifecycle events, results, and progress reporting.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Jobs.Abstractions`](src/Josupeit.Practices.Jobs.Abstractions/README.md) | `IJob`, `IJobWithResult<T>`, `IJobWithProgress<T>`, `IJobFactory` interfaces |
| [`Josupeit.Practices.Jobs.Core`](src/Josupeit.Practices.Jobs/README.md) | `JobBase` abstract base class for custom job implementations |
| [`Josupeit.Practices.Jobs.TaskPool`](src/Josupeit.Practices.Jobs.TaskPool/README.md) | `TaskPoolJobFactory` — fire-and-track jobs on the thread pool |
| [`Josupeit.Practices.Jobs.Scheduler`](src/Josupeit.Practices.Jobs.Scheduler/README.md) | `SchedulerJobFactory` — jobs routed through an `IScheduler` for ordering and bounded concurrency |

### Disposable mixin

Source-generated `IDisposable` / `IAsyncDisposable` implementation with compile-time diagnostics and code fixes.

| Package | Description |
|---|---|
| [`Josupeit.Practices.Mixins.Disposable`](src/Josupeit.Practices.Mixins.Disposable/README.md) | Meta-package — install this one; bundles the generator and analyzer as a dev dependency |
| [`Josupeit.Practices.Mixins.Disposable.Generator`](src/Josupeit.Practices.Mixins.Disposable.Generator/README.md) | Roslyn incremental source generator |
| [`Josupeit.Practices.Mixins.Disposable.Analyzer`](src/Josupeit.Practices.Mixins.Disposable.Analyzer/README.md) | Roslyn analyzer and code fix providers |

## Building and testing

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test
```

## License

[EUPL-1.2](https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12)
