# Josupeit.Practices.Mixins.Disposable

Implementing `IDisposable` correctly is not hard in principle — check whether you have already disposed, release managed resources, release unmanaged resources, suppress finalization — but it is repetitive, and the repetition creates opportunities for subtle bugs: forgetting the idempotency check, not implementing `IAsyncDisposable` alongside `IDisposable`, or calling the wrong overload. When you have dozens of classes that own resources, that boilerplate adds up fast.

This meta-package installs a Roslyn source generator that writes the entire dispose implementation for you. You mark a class with `[DisposableMixin]`, provide methods for the resources you actually own, and the generator produces a correct, idiomatic dispose pattern at compile time — no runtime overhead, no reflection.

```
dotnet add package Josupeit.Practices.Mixins.Disposable
```

The package is marked as a development dependency (`DevelopmentDependency=true`), so it does not appear as a runtime dependency in packages that consume your library.

## How to use

Mark your class as `partial` and apply `[DisposableMixin]`. Then provide override points for the resource categories you own. You do not need to implement `IDisposable` or `IAsyncDisposable` yourself — the generator adds them.

```csharp
using Josupeit.Practices.Mixins.Disposable;

[DisposableMixin]
public partial class DataExporter
{
    private readonly Stream _output = File.Create("export.bin");

    public async Task ExportAsync(IEnumerable<Record> records, CancellationToken cancellationToken)
    {
        ThrowIfDisposed(); // generated guard — call this at the start of every public method
        await WriteRecordsAsync(_output, records, cancellationToken);
    }

    // Override this if you own managed (disposable) resources.
    protected virtual void DisposeManagedResources()
    {
        _output.Dispose();
    }

    // Override this if you own managed resources that support async disposal.
    protected virtual async ValueTask DisposeManagedResourcesAsync()
    {
        await _output.DisposeAsync();
    }

    // Override this if you own unmanaged resources (handles, pointers, etc.).
    // protected virtual void DisposeUnmanagedResources() { ... }
}
```

The generator produces both `IDisposable` and `IAsyncDisposable` implementations by default on platforms that support it (netstandard2.1 and later). This mirrors the behavior of the BCL's own disposable types and avoids the awkwardness of implementing only one interface when callers might need the other.

## Options

Pass a `DisposableMixinOptions` flags value to the attribute to tailor the generated code.

| Flag | Effect |
|---|---|
| `WithProperty` | Generates a public `bool IsDisposed { get; }` property |
| `WithEvent` | Generates a `Disposing` event that is raised when the instance is about to release its resources |
| `WithoutIdempotencyGuarantees` | Omits the double-dispose guard — use only when you can guarantee `Dispose` is called once |
| `WithoutAsyncDisposable` | Generates only `IDisposable`, not `IAsyncDisposable` |

```csharp
// Expose disposal state for UI binding or observability.
[DisposableMixin(DisposableMixinOptions.WithProperty | DisposableMixinOptions.WithEvent)]
public partial class ObservableConnection { … }
```

## Analyzer and code fixes

The package also includes a Roslyn analyzer that catches common mistakes at compile time — for example, forgetting the `partial` keyword, using the wrong visibility on a cleanup method, or applying the mixin to a class that already inherits a disposable base. Most of these diagnostics come with a code fix you can apply directly from the IDE's lightbulb menu. The code fixes can also scaffold the `DisposeManagedResources`, `DisposeManagedResourcesAsync`, and `DisposeUnmanagedResources` method stubs for you.

For the full list of diagnostics and code fixes, see the [Analyzer README](../Josupeit.Practices.Mixins.Disposable.Analyzer/README.md).

For the complete generated member reference and generator limitations, see the [Generator README](../Josupeit.Practices.Mixins.Disposable.Generator/README.md).
