# Josupeit.Practice.Disposables.Mixins

This project is a source-generator that creates a disposable implementation to partial classes decorated with the
`[DisposableMixin]` attribute. Its behavior can be customized.

## Getting started

1. Add the `[DisposableMixin]` attribute to a class.
2. Implement the methods `DisposeManagedResources` and `DisposeManagedResourcesAsync` if the class owns managed
   resources (i.e. other disposable instances).
3. Implement the method `DisposeUnmanagedResources` if the class owns unmanaged resources (e.g. file handles).
4. Ensure to call the method `ThrowIfDisposed` on the public api surface.

## Example

```csharp
[DisposableMixin]
public class MyService
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

The generator creates implementations for `IDisposable` *and* `IAsyncDisposable` by default on supported
platforms. This is an explicit design decision. Nowadays, most of the CLR types implement both interfaces and it
it quite a hazzle to additionaly add an implemenation for either interface when a base class implements only one
(see limitations).

## Limitations

### The mixin works only on classes.

It was considered to add support for structs, but this has been abandoned, because it implies many caveats
that lead to strange behavior:

1. **Structs are being copied.** So if you pass it to a method and inside that method attach an event handler, the handler
    is no longer visible at the call-side. We would have to add a reference to the struct to enable events which
    would imply heap allocations which might be undesired.
2. **Structs can be read-only.** This either disabled idempotency entirely or, again, would imply heap allocations.
3. **Structs can be ref-structs.** These often cannot implement interfaces (dependent to language version).
    
All this creates lots of additional code. In contrast, disposable structs are rarely seen in the wild, so they
are just unsupported.

### The mixin does not work on classes derived from other classes that implement `IDisposable` or `IAsyncDisposable`.

There are dozens of `IDisposable` implementations in the wild. If they are done right (which is not always the case)
they have hooks for deriving classes to cleanup ressources owned by the deriving implementation. However, it's hard
to detect and hook into multiple implementations. Developers should instead utilize these hooks themselves to achieve
the desired behavior. Also, if a base class implements only `IDisposable` or `IAsyncDisposable` and you want to implement
the counterpart it's almost impossible to invoke sync cleanup from an added async implementation and vice versa.

Since these cases are considered edge-cases which required much work to implement, this is just unsupported.
