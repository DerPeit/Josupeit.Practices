# Josupeit.Practices.Mixins.Disposable.Analyzer

The source generator writes the dispose implementation for you, but it cannot write your resource cleanup methods — only you know what your class owns. This analyzer closes that gap. It inspects every class decorated with `[DisposableMixin]` and reports diagnostics when the class is not set up correctly, then offers code fixes that insert the missing pieces directly from your IDE's lightbulb menu.

You do not reference this project directly. Install the meta-package `Josupeit.Practices.Mixins.Disposable` instead; it bundles this analyzer and the companion source generator together as a development dependency.

## Diagnostics

### Errors — the generator will not run

| ID | Title | Trigger |
|---|---|---|
| JPMD010 | Class is already disposable | The class derives from a base that already implements `IDisposable` or `IAsyncDisposable`. The mixin cannot compose with an existing dispose chain. |
| JPMD020 | Class must be partial | The `[DisposableMixin]` class is not declared `partial`. The generator cannot emit a second partial file without it. |
| JPMD021 | Enclosing type must be partial | A class that nests the `[DisposableMixin]` class is itself not `partial`. |

### Warnings — the configuration is likely wrong

| ID | Title | Trigger |
|---|---|---|
| JPMD030 | Sealed class implements no callback | A `sealed` class has `[DisposableMixin]` but overrides none of the three resource cleanup methods. A sealed class that disposes nothing is almost certainly a mistake. |
| JPMD040 | Method should be protected virtual | A resource cleanup method exists but does not have `protected virtual` visibility. The generator expects this signature on non-sealed classes so that derived classes can extend the cleanup. |
| JPMD041 | Method should be private | A resource cleanup method on a `sealed` class is not `private`. Because the class cannot be derived, `protected virtual` is unnecessary and misleading. |

### Info — the class implements no cleanup

| ID | Title | Trigger |
|---|---|---|
| JPMD031 | Class implements no callback | A non-sealed class has `[DisposableMixin]` but overrides none of the three resource cleanup methods. This is reported as info rather than a warning because a non-sealed class may be a base class whose subclasses do the actual cleanup. |

### Hidden — used to anchor code fixes

The following diagnostics are hidden (invisible in the editor by default). They exist solely to give the IDE a location at which to offer a code fix that scaffolds the missing method.

| ID | Title | Trigger |
|---|---|---|
| JPMD032 | Class does not implement DisposeManagedResources | `DisposeManagedResources` is absent. |
| JPMD033 | Class does not implement DisposeManagedResourcesAsync | `DisposeManagedResourcesAsync` is absent. |
| JPMD034 | Class does not implement DisposeUnmanagedResources | `DisposeUnmanagedResources` is absent. |

## Code fixes

Every diagnostic that has an obvious mechanical correction comes with a code fix you can apply from the lightbulb menu.

| Fix | Diagnostic | What it does |
|---|---|---|
| Add `partial` keyword | JPMD020, JPMD021 | Inserts `partial` into the type's modifier list. |
| Implement `DisposeManagedResources` | JPMD032 | Appends a `protected virtual void DisposeManagedResources()` stub with an XML doc comment (`private` instead of `protected virtual` on sealed classes). |
| Implement `DisposeManagedResourcesAsync` | JPMD033 | Appends an `async protected virtual ValueTask DisposeManagedResourcesAsync()` stub (same sealed-class adjustment). |
| Implement `DisposeUnmanagedResources` | JPMD034 | Appends a `protected virtual void DisposeUnmanagedResources()` stub (same sealed-class adjustment). |
| Make method `protected virtual` | JPMD040 | Replaces the method's visibility modifiers with `protected virtual`. |
| Make method `private` | JPMD041 | Replaces the method's visibility modifiers with `private`. |
