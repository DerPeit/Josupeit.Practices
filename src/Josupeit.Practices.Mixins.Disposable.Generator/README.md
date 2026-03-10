# Josupeit.Practices.Mixins.Disposable.Generator

This is the Roslyn incremental source generator that powers `[DisposableMixin]`. It reads annotated class declarations at compile time and emits a `partial` class file containing the full `IDisposable` and `IAsyncDisposable` implementation — no runtime reflection, no base class requirement.

You do not reference this project directly. Install the meta-package `Josupeit.Practices.Mixins.Disposable` instead; it bundles this generator and the companion analyzer together as a development dependency.

## Getting started

1. Add `[DisposableMixin]` to a `partial` class.
2. Override `DisposeManagedResources` and/or `DisposeManagedResourcesAsync` for managed resources (other disposable instances).
3. Override `DisposeUnmanagedResources` for unmanaged resources such as file handles or raw pointers.
4. Call `ThrowIfDisposed()` at the start of every public method to guard against post-disposal use.

## Example

```csharp
[DisposableMixin]
public partial class MyService
{
    private readonly Stream _tempFile = File.OpenWrite(Path.GetTempFileName());

    public async Task SayHelloAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        await _tempFile.WriteAsync("Hello world!"u8.ToArray(), cancellationToken);
    }

    protected virtual void DisposeManagedResources()
    {
        _tempFile.Dispose();
    }

    protected virtual async ValueTask DisposeManagedResourcesAsync()
    {
        await _tempFile.DisposeAsync();
    }
}
```

## Behavior

The generator produces implementations for both `IDisposable` and `IAsyncDisposable` by default on platforms that support it (netstandard2.1 and later). This is an explicit design decision. Most BCL types implement both interfaces, and having only one of them forces callers into awkward synchronous fallbacks or `async void` hacks. Generating both by default avoids that problem entirely.

## Limitations

### The mixin works only on classes

Struct support was considered and rejected because of several unavoidable issues:

1. **Structs are copied.** Event handlers attached inside a called method become invisible at the call site because they operate on a copy.
2. **Structs can be read-only.** Supporting idempotency on a read-only struct would require heap allocation.
3. **Structs can be ref structs.** These often cannot implement interfaces at all, depending on the language version.

Disposable structs are rare enough in practice that they are simply unsupported.

### The mixin does not work on classes that already inherit a disposable base

When a base class already implements `IDisposable` or `IAsyncDisposable`, that base class provides hooks (typically `protected virtual void Dispose(bool)`) for derived classes to release their own resources. The generator cannot reliably detect and integrate with the many different hook patterns that exist in the wild. If you are in this situation, use the base class's existing hook directly rather than applying `[DisposableMixin]`.

For diagnostics and code fixes related to these rules, see the [Analyzer README](../Josupeit.Practices.Mixins.Disposable.Analyzer/README.md).
