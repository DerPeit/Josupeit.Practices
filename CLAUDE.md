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

## Architecture

This is a .NET class library solution providing reusable patterns. It is organized into `src/` (18 library projects) and `test/` (5 test projects).

### Multi-targeting

Libraries target netstandard2.0 as minimum, with netstandard2.1 where IAsyncDisposable support is needed. Schedulers.Channel additionally targets netcoreapp3.0 to avoid an unnecessary System.Threading.Channels NuGet dependency. Framework-specific behavior uses conditional compilation. Test projects target equivalence classes of these frameworks (e.g. net481 covers netstandard2.0 on .NET Framework, netcoreapp2.2 covers netstandard2.0 on .NET Core, net80 covers netstandard2.1).

### Code Generation

**T4 Templates:** Several projects (Services, Schedulers, Jobs) use `.tt` text templates that produce `*.g.cs` files. These are transformed at build time via `dotnet t4` (configured in `src/Directory.Build.targets`). Generated files are git-ignored.

**Roslyn Source Generator (Disposable Mixins):** The `Mixins.Disposable.Generator` project is an incremental source generator that auto-implements `IDisposable`/`IAsyncDisposable` on classes decorated with `[DisposableMixin]`. It has a multi-stage rewriting pipeline in `Generation/` and semantic analysis in `Analysis/`. The companion `Mixins.Disposable.Analyzer` project provides compile-time diagnostics. The `Mixins.Disposable` project is the NuGet package wrapper that bundles both as a development dependency. The analyzer references the generator as `<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false">`, so the generator runs on the analyzer's own compilation and emits the marker attributes into it - that is how the analyzer can use `typeof(DisposableMixinAttribute)` without referencing the generator assembly. The `Mixins.Disposable.Tests` project is a regular NUnit project (net80, net10.0) that drives the generator through `CSharpGeneratorDriver` and the analyzer and code fixes through the `Microsoft.CodeAnalysis.*.Testing` verifiers in `Verifiers/`.

### Library Modules

- **Disposables / Disposables.Abstractions** — Explicit and implicit `IDisposable`/`IAsyncDisposable` base classes with GC suppression.
- **Commands / Commands.Abstractions** — CQRS-style command/query pattern with recursion prevention (`Executable` base).
- **Services / Services.Abstractions** — Sync and async service abstractions (T4-generated overloads).
- **Schedulers / Schedulers.Abstractions** — Type-safe scheduler with sync/async/cancellable variants (T4-generated). Includes TaskPool and Channel implementations.
- **Jobs / Jobs.Abstractions** — Background job execution (T4-generated). Includes TaskPool and Scheduler implementations.
- **Specifications** — Specification pattern.

## Testing Conventions

- **Framework:** NUnit 4 (`[Test]`, `[TestFixture]`, `Assert.That` constraint model), except on `netcoreapp2.2`, which is pinned to NUnit 3.14.0 in `test/Directory.Build.props` because NUnit 4 tooling no longer supports it. Shared test code therefore has to compile and pass under **both** majors - avoid APIs that exist in only one of them. Use BCL delegate types (`Action`, `Func<Task>`) rather than NUnit's `TestDelegate` and `AsyncTestDelegate`, which NUnit 4 deprecated; `Assert.That` and the `Throws` constraints accept them on both versions.
- **Mocking:** NSubstitute — use `Substitute.ForPartsOf<T>()` for partial mocks of abstract classes, `Substitute.For<T>()` for interfaces.
- **Test class naming:** `{ClassUnderTest}Tests` (e.g. `CommandTests`, `ExplicitDisposableTests`). File names match class names.
- **Test method naming:** Sentence-style with underscores: `{Method}_does_something_when_condition` (e.g. `Execute_calls_overridden_ExecuteAsync`, `Dispose_sets_IsDisposed_to_true`).
- **`[TestOf]` attribute:** Every test class is decorated with `[TestOf(typeof(ClassUnderTest))]`.
- **Test arrangement:** Each test arranges its own state inline where possible. Tests for stateful components with lifecycle (e.g. `ChannelScheduler`) may use `[SetUp]`/`[TearDown]` to manage start/stop.
- **Assertions:** Use NUnit's constraint model (`Assert.That(…, Is.…)`, `Throws.InstanceOf<T>()`, `Throws.Nothing`). Legacy `Assert.Throws<T>` / `Assert.ThrowsAsync<T>` are also used where appropriate.
- **Async tests:** Return `Task` and use `async`/`await` directly.
- **Generic test inheritance:** Some test suites use a generic base class pattern — a concrete class inherits from an abstract `Tests<TService>` to test a type hierarchy (e.g. `ServiceTests : ServiceTests<Service>`).
- **Helper classes:** A shared `TestException : Exception` class per test project for exception-path testing. `SubstituteExtensions` helper classes provide reflection-based `.Protected()` / `.InvokeMember()` extensions for verifying protected member calls via NSubstitute.

## Branching & Release Workflow

### Merging

Always use `--no-ff` when merging feature branches to `develop` and when merging `develop` to `main`.

### Releasing to main

After merging `develop` into `main`, follow these steps:

1. **Dry-run version tags** to see which packages are being incremented:
   ```bash
   dotnet nbgv tag --what-if -p src/Josupeit.Practices.SomeProject
   ```
   Run this for each project that has new commits since its last tag. Each invocation prints a line like `PackageName/v1.2.3`.

2. **Reword the merge commit** to follow the release convention:
   ```bash
   git commit --amend -m "chore(release): PackageA, PackageB 1.2"
   ```
   Use the short package names (without the `Josupeit.Practices.` prefix) and the major.minor version when all packages share the same version. When packages have different versions, list each with its version.

3. **Create the version tags** for real:
   ```bash
   dotnet nbgv tag -p src/Josupeit.Practices.SomeProject
   ```
   Run this for each project that was included in the release.

### nbgv invocation notes

- nbgv is a **local dotnet tool** - always invoke it as `dotnet nbgv`, not bare `nbgv`.
- `dotnet tool restore` must have been run at least once in the repo for `dotnet nbgv` to work.
- The `-p` flag takes the **project directory path** (e.g. `src/Josupeit.Practices.Commands`), not a `.csproj` file path.
- Use `--what-if` with the `tag` subcommand for dry-runs; it prints the tag name without creating it.

## Package Versions

Most `PackageReference` versions in `src/Directory.Build.props` and `test/Directory.Build.props` float (`Version="*"`). Resolution is pinned by a `packages.lock.json` per project, committed alongside it, and `RestoreLockedMode` is enabled when `$(CI) == 'true'` so CI fails instead of silently drifting.

- A floating version resolving to something new does **not** change the build until the lock files are regenerated. That is deliberate - every version bump is an explicit, reviewable commit.
- To take newer packages, run `dotnet restore --force-evaluate` and commit the resulting lock file changes.
- `error NU1004` or `NU1403` on CI means the lock files are out of date or were hand-edited. Regenerate them, do not delete them.

## Conventions

- Follow the rules in [CODING_STANDARDS.md](CODING_STANDARDS.md) for naming, style, member ordering, and general practices.
- C# latest language version, nullable reference types enabled, implicit usings disabled.
- Abstractions are in separate `*.Abstractions` projects with interface-only definitions.
- Use American English spelling (e.g. "canceled", not "cancelled").
- License: EUPL-1.2.
