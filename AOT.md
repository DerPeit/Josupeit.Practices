# Native AOT Compatibility

The packable runtime libraries in this repository are **Native AOT compatible**. On their `net8.0`
target they are marked `<IsAotCompatible>true</IsAotCompatible>`, which turns on the full trim / AOT /
single-file analyzer suite at build time, so AOT-hostile code is flagged during compilation rather
than failing at publish.

## What "AOT-compatible" means here

- **The libraries build analyzer-clean.** A Release build produces zero `IL` (trim/AOT/single-file)
  warnings on the `net8.0` target.
- **No quarantined APIs.** There are no `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`
  attributes on the public surface. No feature is walled off as AOT-unsafe.
- **A real native binary using the libraries runs.** A smoke-test app is published with
  `PublishAot=true` and executed in CI on every push (see below).

It does **not** mean your application is automatically AOT-compatible. If your own code uses
reflection, dynamic code generation, or reflection-based serialization, that remains your
responsibility. These libraries will not be the thing that breaks your AOT publish.

## Multi-targeting

Every packable runtime library still targets `netstandard2.0` as its minimum (some also
`netstandard2.1`, one also `netcoreapp3.0`). The `net8.0` target is added purely to carry the AOT
analyzers and the AOT-safe code paths. A `net8.0`-or-later application that references these packages
resolves the `net8.0` asset, which is the AOT-verified one. Older consumers keep the netstandard
assets unchanged.

The Roslyn source generator and analyzer (`Mixins.Disposable.*`) stay `netstandard2.0`-only: they run
inside the compiler at build time and never ship into a consumer's runtime, so AOT does not apply to
them.

## How it's verified

The repository ships a Native AOT regression fixture at `aot/Josupeit.Practices.AotSmoke/`. It is
**not** a usage example: it is a CI gate.

`Josupeit.Practices.AotSmoke.csproj` sets:

```xml
<PublishAot>true</PublishAot>
<IlcTreatWarningsAsErrors>true</IlcTreatWarningsAsErrors>
<TrimmerSingleWarn>false</TrimmerSingleWarn>
```

so any trim/AOT `IL` warning in a reachable code path **fails the publish**, and each warning is
reported individually rather than collapsed. The CI `aot` job (`.forgejo/workflows/ci.yml`):

1. Installs the native toolchain (`clang`, `zlib1g-dev`).
2. Publishes `AotSmoke` as a native binary (`dotnet publish --runtime linux-x64`).
3. Runs the native binary and checks it prints `AOT SMOKE OK`.

If any step fails, CI fails.

### What the smoke test exercises

The smoke test invokes the primary public API of every packable runtime library under the native
binary, asserting each path actually ran:

- **Specifications** composed with the `&`, `|`, and `!` operators, evaluated sync and async.
- **Commands and queries**, including the async bridge over a sync override.
- **Services**, both the sync `Service` and the `AsyncService` lifecycle.
- **Schedulers** (`TaskPoolScheduler` and `ChannelScheduler`) scheduling sync and async work.
- **Jobs** (`TaskPoolJobFactory` and `SchedulerJobFactory`) with progress reporting and results.

The scheduler paths are the important ones: scheduling work without an explicit `ExecutionContext`
routes through the one internal detail below.

## The one internal detail: `ExecutionContext.Default`

The schedulers run callbacks under `ExecutionContext.Default` (the pristine, empty ambient context)
when the caller supplies none, so ambient `AsyncLocal` state does not leak into scheduled work.
`ExecutionContext.Default` is an internal field with no public accessor, and it must be non-null
because `ExecutionContext.Run` rejects null (unlike `Capture`, which returns null for a clean
context).

On `net8.0` and later this field is bound with `[UnsafeAccessor]`, which the trimmer and AOT compiler
fully understand and preserve. No reflection or dynamic code is involved. On netstandard, where
`UnsafeAccessor` does not exist, a plain reflection fallback is used; those targets are not
AOT-published.

## Tips for AOT-publishing your own app

- Keep your own code reflection-free, or annotate it with the standard `[RequiresUnreferencedCode]` /
  `[RequiresDynamicCode]` attributes.
- Set `<IsAotCompatible>true</IsAotCompatible>` in your own project to get the analyzers, then fix
  warnings as they appear.
- Test with an actual `dotnet publish -p:PublishAot=true` run, not just a build. Some issues only
  surface during native compilation.
