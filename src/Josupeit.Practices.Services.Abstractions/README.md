# Josupeit.Practices.Services.Abstractions

Background services tend to share the same lifecycle: they start, optionally pause and resume, and eventually stop. This package captures that lifecycle as a pair of interfaces — one synchronous, one asynchronous — so that the rest of your application can control a service without knowing anything about its concrete implementation.

```
dotnet add package Josupeit.Practices.Services.Abstractions
```

## The lifecycle

A service always begins in the `Stopped` state and transitions through the following states:

```
Stopped → Starting → Running → Stopping → Stopped
                             ↘ Pausing → Paused → Resuming → Running
```

The `ServiceState` enum exposes every one of these states, which makes it straightforward to reflect the current status in a UI or to write guards in the service implementation itself.

## `IService` — synchronous lifecycle

```csharp
public interface IService
{
    ServiceState State { get; }

    void Start();
    void Stop();

    // Pause and Resume throw NotSupportedException when the implementation does not support pausing.
    void Pause();
    void Resume();
}
```

## `IAsyncService` — asynchronous lifecycle

`IAsyncService` mirrors `IService` exactly, but every operation returns a `Task`. Each method is available in two overloads — one without a `CancellationToken` and one with — so callers can choose the level of control they need.

```csharp
Task StartAsync();
Task StartAsync(CancellationToken cancellationToken);
Task StopAsync();
Task StopAsync(CancellationToken cancellationToken);
Task PauseAsync();
Task PauseAsync(CancellationToken cancellationToken);
Task ResumeAsync();
Task ResumeAsync(CancellationToken cancellationToken);
```

## When to use this package directly

Reference `Josupeit.Practices.Services.Abstractions` in any layer that needs to start, stop, or observe a service but should not depend on how it is implemented. For the concrete base classes that manage state transitions for you, install `Josupeit.Practices.Services` instead.
