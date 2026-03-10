# Josupeit.Practices.Services

Managing a service lifecycle correctly is surprisingly subtle. State transitions need to be valid (you cannot resume a service that was never paused), thread safety matters, and disposal has to be coordinated with the running state. This package provides abstract base classes that handle all of that bookkeeping, leaving you to focus on what the service actually does.

```
dotnet add package Josupeit.Practices.Services
```

## How to implement a service

### Synchronous service

Inherit from `Service` and implement the two abstract lifecycle hooks. The base class manages the `State` property and validates transitions — if you call `Start()` on an already-running service, it throws an `InvalidOperationException` rather than silently misbehaving.

```csharp
public sealed class DataSyncService : Service
{
    private Timer? _timer;

    protected override void OnStart()
    {
        _timer = new Timer(_ => Synchronize(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    protected override void OnStop()
    {
        _timer?.Dispose();
        _timer = null;
    }
}

var svc = new DataSyncService();
svc.Start();
Console.WriteLine(svc.State); // Running
svc.Stop();
```

### Adding pause support

Pausing is opt-in. Override `CanPause` to return `true`, then override the two pause hooks.

```csharp
public sealed class ThrottledSyncService : Service
{
    public override bool CanPause => true;

    protected override void OnStart()  { /* start work */ }
    protected override void OnStop()   { /* stop work  */ }
    protected override void OnPause()  { /* stop processing new items */ }
    protected override void OnResume() { /* resume processing         */ }
}
```

### Asynchronous service

When the lifecycle itself involves asynchronous operations — for example, flushing a queue before stopping — inherit from `AsyncService` instead. The hooks receive a `CancellationToken` that callers can use to abort a long-running transition.

```csharp
public sealed class BackgroundProcessor : AsyncService
{
    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
    }

    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        await FlushAsync(cancellationToken);
    }
}

var worker = new BackgroundProcessor();
await worker.StartAsync(cancellationToken);
await worker.StopAsync(cancellationToken);
```

### Service with disposal

When the service owns resources that must be released, choose a base class that also implements `IDisposable` or `IAsyncDisposable`. Override the resource cleanup methods to release what the service owns.

```csharp
public sealed class ResourceService : DisposableService
{
    private readonly Stream _stream = File.OpenRead("data.bin");

    protected override void OnStart() { /* ... */ }
    protected override void OnStop()  { /* ... */ }

    protected override void DisposeManagedResources() => _stream.Dispose();
}
```

## Available base classes

| Base class | Lifecycle | Disposal |
|---|---|---|
| `Service` | Sync | — |
| `DisposableService` | Sync | `IDisposable` |
| `AsyncDisposableService` | Sync | `IDisposable` + `IAsyncDisposable` |
| `AsyncService` | Async | — |
| `DisposableAsyncService` | Async | `IDisposable` |
| `AsyncDisposableAsyncService` | Async | `IDisposable` + `IAsyncDisposable` |

The interfaces that these classes implement live in `Josupeit.Practices.Services.Abstractions`, which is pulled in automatically as a transitive dependency.
