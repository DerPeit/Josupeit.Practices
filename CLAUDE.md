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

This is a .NET class library solution providing reusable patterns. It is organized into `src/` (16 library projects) and `test/` (10 test projects).

### Multi-targeting

Libraries target netstandard2.0 as minimum, with netstandard2.1 where IAsyncDisposable support is needed. Schedulers.Channel additionally targets netcoreapp3.0 to avoid an unnecessary System.Threading.Channels NuGet dependency. Framework-specific behavior uses conditional compilation. Test projects target equivalence classes of these frameworks (e.g. net481 covers netstandard2.0 on .NET Framework, netcoreapp2.2 covers netstandard2.0 on .NET Core, net80 covers netstandard2.1).

### Code Generation

**T4 Templates:** Several projects (Services, Schedulers, Jobs) use `.tt` text templates that produce `*.g.cs` files under each target framework's `obj/` directory. They are transformed at build time via `dotnet t4` (configured in `src/Directory.Build.targets`), once per target framework so that parallel builds never share a generated file. Generated files are git-ignored.

**Roslyn Source Generator (Disposable Mixins):** The `Mixins.Disposable.Generator` project is an incremental source generator that auto-implements `IDisposable`/`IAsyncDisposable` on classes decorated with `[DisposableMixin]`. It has a multi-stage rewriting pipeline in `Generation/` and semantic analysis in `Analysis/`. The companion `Mixins.Disposable.Analyzer` project provides compile-time diagnostics. The `Mixins.Disposable` project is the NuGet package wrapper that bundles both as a development dependency. The analyzer references the generator as `<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false">`, so the generator runs on the analyzer's own compilation and emits the marker attributes into it - that is how the analyzer can use `typeof(DisposableMixinAttribute)` without referencing the generator assembly. The `Mixins.Disposable.Tests` project is a regular NUnit project (net80, net10.0) that drives the generator through `CSharpGeneratorDriver` and the analyzer and code fixes through the `Microsoft.CodeAnalysis.*.Testing` verifiers in `Verifiers/`.

### Library Modules

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

### Merging feature branches

1. **Push the branch and create a PR** on Codeberg targeting `develop`.
2. **Wait for CI** (build + AOT pipeline) to go green.
3. **Complete the PR** on Codeberg (--no-ff merge).
4. **Pull and clean up locally:**
   ```bash
   git checkout develop
   git pull
   git branch -d feature/branch-name
   ```

### Releasing to main

After merging `develop` into `main` with `--no-ff`, follow these steps:

1. **Dry-run version tags** to see which packages are being incremented:

   ```bash
   dotnet nbgv tag --what-if -p src/Josupeit.Practices.SomeProject
   ```

   Run this for each project that has new commits since its last tag. Each invocation prints a line like `PackageName/v1.2.3`.

2. **Ship the public API surface** for each released project: move all entries from `PublicAPI.Unshipped.txt` into `PublicAPI.Shipped.txt` (sorted alphabetically), leaving only the `#nullable enable` header in the Unshipped file. Do the same for any TFM-specific files (`PublicAPI.Unshipped.{tfm}.txt` -> `PublicAPI.Shipped.{tfm}.txt`). Stage the changes - the next step's `--amend` folds them into the merge commit.

3. **Reword the merge commit** to follow the release convention:

   ```bash
   git commit --amend -m "chore(release): PackageA, PackageB 1.2"
   ```

   Use the short package names (without the `Josupeit.Practices.` prefix) and the major.minor version when all packages share the same version. When packages have different versions, list each with its version.

4. **Create the version tags** for real:
   ```bash
   dotnet nbgv tag -p src/Josupeit.Practices.SomeProject
   ```
   Run this for each project that was included in the release.

### nbgv invocation notes

- nbgv is a **local dotnet tool** - always invoke it as `dotnet nbgv`, not bare `nbgv`.
- `dotnet tool restore` must have been run at least once in the repo for `dotnet nbgv` to work.
- The `-p` flag takes the **project directory path** (e.g. `src/Josupeit.Practices.Commands`), not a `.csproj` file path.
- Use `--what-if` with the `tag` subcommand for dry-runs; it prints the tag name without creating it.

## Public API Tracking

Every packable `src/` project ships with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`, enforced by `Microsoft.CodeAnalysis.PublicApiAnalyzers`. The three Mixins.Disposable projects opt out via `<EnablePublicApiAnalyzers>false</EnablePublicApiAnalyzers>` because they have no public API surface of their own. T4-generated files restore RS0016 and RS0017 right after their blanket `#pragma warning disable` (see the `.tt` headers), so generated members are tracked exactly like hand-written ones.

- **Adding a public member**: add its signature to `PublicAPI.Unshipped.txt`. The build emits RS0016 if it is missing. Bump the project's minor version in its `version.json` before the release: dependents declare `[major.minor, next-major)` (the `~` placeholder in `src/Directory.Build.targets`), so a member added in a patch release is missing for a consumer that resolves the lowest patch.
- **Removing a public member**: prefix the line with `*REMOVED*` in the relevant file. The build emits RS0017 if a declared member no longer exists.
- **Releasing**: move all Unshipped entries into Shipped (sorted), leaving only the `#nullable enable` header. See the release workflow above.

## LLM Documentation

Every packable `src/` project carries `llms.txt` and `llms-full.txt` (llmstxt.org format), packed into the NuGet next to the README. `src/Josupeit.Practices.Commands.Abstractions/` is the reference implementation; new or updated files follow its structure.

- **`llms.txt`** is the index: H1 package name, blockquote summary, a heading-free body with namespace, install command, and type list, then `## Documentation` (llms-full.txt, PublicAPI.Shipped.txt where present, README), `## Related packages`, and `## Optional`.
- **`llms-full.txt`** is the self-contained reference, in this section order: At a glance (package, namespace, target frameworks, Native AOT, dependencies, contents, nullability note, PublicAPI link, where this document lives, implementations, license, install command), Concepts, the type catalog (summary table, then one `###` per type with its exact C# declaration), Variance where generics exist, Contract expectations, Usage, a choosing/decision guide, Common mistakes, Related packages, License.
- Declarations are written as C# under `#nullable enable`, never in the `PublicAPI.Shipped.txt` format. That file is linked as the machine-verified inventory, never inlined. Projects with per-TFM API files link those as well and explain that the base file is the intersection and the per-TFM files are additions.
- Common mistakes lists the errors an LLM predictably makes against the package: wrong namespace, MediatR-style assumptions, reversed type parameters, non-existent overloads, partial implementations.
- An Abstractions pair describes the contract only and states explicitly where the contract leaves behavior open. What the base classes actually do belongs in the implementation package's pair, reached by link - never asserted as if it were part of the interface. Relations the contract does define are stated as requirements, never softened to conventions. Keep concrete API facts out of this file: subagents receive it in their system context, so any fact stated here contaminates a probe that asks about it.
- Related packages links the counterpart package in both directions - abstractions and implementation each point at the other's `llms-full.txt` - plus the repository overview.
- Links to repository files are absolute raw Codeberg URLs on `main` (`https://codeberg.org/DerPeit/Josupeit.Practices/raw/branch/main/...`); the source-tree link in `## Optional` uses the web UI URL. Never link `PublicAPI.Unshipped.txt`.
- The blockquote summary is identical in both files.
- Both files are formatted by prettier as markdown (`.prettierrc` override) with CRLF line endings (`.editorconfig` `[*.txt]`). Run `npx prettier --write "**/llms.txt" "**/llms-full.txt"` after editing.
- Before committing a new or changed pair, compile every snippet: a scratch project outside the repo that references the package, every documented-as-compiling snippet with stub types in one build that must succeed, and one negative build per documented-as-failing line (each behind its own `#if` define) that must fail with the predicted error code. Do not bundle failing lines into one file - a failed `using` suppresses body diagnostics for that compilation unit. For implementation packages, add runtime checks that exercise the library's own types for every behavioral claim; hand-written examples only prove themselves.
- The repository root carries `llms.txt` (the index across all packages, linking every package's `llms-full.txt`) and `context7.json` (Context7 parsing configuration and the family-wide rules). A new package is added to both.
- `scripts/check-llms-docs.sh` is the drift guard and runs as the first CI step: every packable project has its pair (or names its documenting package in `<LlmsDocumentedBy>`), H1 equals the PackageId, the blockquote is identical in both files, every repository link resolves on `main`, every type and member in the PublicAPI files is mentioned in `llms-full.txt`, and the root `llms.txt` links every pair. Line endings are not checked because the repository stores LF and the working copy follows the platform. Run it locally before committing a pair; a public API change that the documentation does not mention fails the build.
- Before committing a new or changed pair, probe it with a context-free agent: a fresh general-purpose agent that may read only that package's `llms-full.txt` (copy it to a neutral path first), answering the package's trap questions - namespace, exact signatures, type parameter order, non-existent overloads, variance, MediatR-style assumptions - and marking `DOC SILENT` wherever the file does not answer. Every `DOC SILENT` is a gap to close. A README-only control run shows which answers the doc actually changed.

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
