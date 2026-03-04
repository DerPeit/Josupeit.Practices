# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Bash Commands

- `cd` into the project directory first, then run git/dotnet commands as separate invocations. Don't use compound commands (`cd ... && git ...`) — they break permission pattern matching.

## Build & Test Commands

```bash
# Build entire solution
dotnet build

# Build a specific project
dotnet build src/Josupeit.Practices.Disposables/Josupeit.Practices.Disposables.csproj

# Run all tests
dotnet test

# Run tests for a specific project
dotnet test test/Josupeit.Practices.Disposables.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~MyTestName"

# Restore local tools (needed for T4 template transformation)
dotnet tool restore
```

NuGet packages are generated on build (`GeneratePackageOnBuild`).

## Architecture

This is a .NET class library solution providing reusable patterns. It is organized into `src/` (18 library projects) and `test/` (5 test projects).

### Multi-targeting

Libraries target netstandard2.0 as minimum, with netstandard2.1 where IAsyncDisposable support is needed. Schedulers.Channel additionally targets netcoreapp3.0 to avoid an unnecessary System.Threading.Channels NuGet dependency. Framework-specific behavior uses conditional compilation. Test projects target equivalence classes of these frameworks (e.g. net481 covers netstandard2.0 on .NET Framework, netcoreapp2.2 covers netstandard2.0 on .NET Core, net80 covers netstandard2.1).

### Code Generation

**T4 Templates:** Several projects (Services, Schedulers, Jobs) use `.tt` text templates that produce `*.Generated.cs` files. These are transformed at build time via `dotnet t4` (configured in `src/Directory.Build.targets`). Generated files are git-ignored.

**Roslyn Source Generator (Disposable Mixins):** The `Mixins.Disposable.Generator` project is an incremental source generator that auto-implements `IDisposable`/`IAsyncDisposable` on classes decorated with `[DisposableMixin]`. It has a multi-stage rewriting pipeline in `Generation/` and semantic analysis in `Analysis/`. The companion `Mixins.Disposable.Analyzer` project provides compile-time diagnostics. The `Mixins.Disposable` project is the NuGet package wrapper that bundles both as a development dependency. The `Mixins.Disposable.Tests` project is *not* a runtime test project — it's a netstandard2.0 compilation-only project that verifies the generator/analyzer output by referencing them as `<ProjectReference OutputItemType="Analyzer">`.

### Library Modules

- **Disposables / Disposables.Abstractions** — Explicit and implicit `IDisposable`/`IAsyncDisposable` base classes with GC suppression.
- **Commands / Commands.Abstractions** — CQRS-style command/query pattern with recursion prevention (`Executable` base).
- **Services / Services.Abstractions** — Sync and async service abstractions (T4-generated overloads).
- **Schedulers / Schedulers.Abstractions** — Type-safe scheduler with sync/async/cancellable variants (T4-generated). Includes TaskPool and Channel implementations.
- **Jobs / Jobs.Abstractions** — Background job execution (T4-generated). Includes TaskPool and Scheduler implementations.
- **Specifications** — Specification pattern.

## Testing Conventions

- **Framework:** NUnit 3 (`[Test]`, `[TestFixture]`, `Assert.That` constraint model).
- **Mocking:** NSubstitute — use `Substitute.ForPartsOf<T>()` for partial mocks of abstract classes, `Substitute.For<T>()` for interfaces.
- **Test class naming:** `{ClassUnderTest}Tests` (e.g. `CommandTests`, `ExplicitDisposableTests`). File names match class names.
- **Test method naming:** Sentence-style with underscores: `{Method}_does_something_when_condition` (e.g. `Execute_calls_overridden_ExecuteAsync`, `Dispose_sets_IsDisposed_to_true`).
- **`[TestOf]` attribute:** Every test class is decorated with `[TestOf(typeof(ClassUnderTest))]`.
- **No `[SetUp]`/`[TearDown]`:** Each test arranges its own state inline (no shared fixtures or setup methods).
- **Assertions:** Use NUnit's constraint model (`Assert.That(…, Is.…)`, `Throws.InstanceOf<T>()`, `Throws.Nothing`). Legacy `Assert.Throws<T>` / `Assert.ThrowsAsync<T>` are also used where appropriate.
- **Async tests:** Return `Task` and use `async`/`await` directly.
- **Generic test inheritance:** Some test suites use a generic base class pattern — a concrete class inherits from an abstract `Tests<TService>` to test a type hierarchy (e.g. `ServiceTests : ServiceTests<Service>`).
- **Helper classes:** A shared `TestException : Exception` class per test project for exception-path testing. `SubstituteExtensions` helper classes provide reflection-based `.Protected()` / `.InvokeMember()` extensions for verifying protected member calls via NSubstitute.

## Conventions

- Follow the rules in [CODING_STANDARDS.md](CODING_STANDARDS.md) for naming, style, member ordering, and general practices.
- C# latest language version, nullable reference types enabled, implicit usings disabled.
- Abstractions are in separate `*.Abstractions` projects with interface-only definitions.
- License: EUPL-1.2.
