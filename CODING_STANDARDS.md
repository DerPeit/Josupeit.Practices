# Coding Standards

## General
- **Readability:** Code is read more often than being written. Ensure to write clear, self-explaining code. Use self-explaining member names. Stick to Clean Code Principles and Practices
- **Consistency:** Stick to consistent naming conventions and code formatting throughout the whole solution
- **Comments:** Only write comments, if the code cannot be self-explaining. Only write comments that explain _why_ the code is like it is. Avoid comments that repeat code
- **Exception handling:** Use structured exception handling. Avoid swallowing exceptions
- **Async/Await:** Use asynchronous programming patterns, where appropriate. Especially when asynchronous overloads exist. Look for CancellationTokens. Avoid async void.
- **Magic values:** Avoid magic numbers and magic strings. Use constants, enums and static readonly fields
- **Organization:** One type per file
- **Argument validation:** Validate arguments of all externally visible methods (public, protected, internal, protected internal — but not private protected). At minimum, throw `ArgumentNullException` for non-nullable reference type parameters. Use a style appropriate for the target framework: prefer `ArgumentNullException.ThrowIfNull` and similar `ThrowIf*` helpers on modern targets (net6.0+); C# 14 extension members may be used to provide these helpers on older targets as well. **Consistency within a method is mandatory:** if any guard in a method requires an `if` block (e.g. `if (value < 0) throw new ArgumentOutOfRangeException(…)`), then all guards in that method must use explicit `if`/`throw` statements — do not mix `ThrowIf*` one-liners with `if`/`throw` blocks in the same method.

## Naming Conventions
- **Types:** PascalCase. Interfaces start with I
- **Methods, properties, events, delegates:** PascalCase
- **Variables, parameters, fields, constants:** camelCase for private visibility, otherwise PascalCase
- **Private fields:** Prefix with an underscore _
- **Member access:** Avoid using this
- **Test methods:** Use descriptive names that clearly convey the scenario. Snake case is allowed.

## Code Style
- **Braces:** Allman style.
- **Indentation:** Spaces, not tabs. 4 spaces per indentation level.
- **Using directives:** Place using directives outside the namespace.
- **Namespace declaration:** Use file-scoped namespaces.
- **Type inference:** Prefer `var` when the type is apparent from context.
- **Access modifiers:** Always state access modifiers explicitly, including `private`.
- **Expression-bodied members:** Allowed when the expression is simple and readable. Do not force complex logic into an expression body; readability takes precedence.
- **Inheritance:** Prefer `sealed` for `internal` types.
- **Asynchronous context:** Always call `ConfigureAwait` on awaited tasks and express intent explicitly — use `ConfigureAwait(false)` in library code that does not need the synchronization context, `ConfigureAwait(true)` when the context is required.

### Modifier Order
Apply modifiers in the following order (matches IDE0036 / ReSharper default):

`public`, `internal`, `protected`, `private`, `file`, `static`, `extern`, `new`, `virtual`, `abstract`, `sealed`, `override`, `readonly`, `unsafe`, `required`, `volatile`, `async`

### Member Order Within a Type
Order members first by kind, then by visibility within each kind group. Within each group, static members precede instance members.

**By kind:**
1. Delegates
2. Enums
3. Constants
4. Static readonly fields
5. Static fields
6. Readonly fields
7. Fields
8. Constructors and finalizers (static constructor first)
9. Events
10. Indexers
11. Properties
12. Operators
13. Methods
14. Nested types (structs, classes)

**By visibility within each kind group:**
1. public
2. internal
3. protected internal
4. protected
5. private protected
6. private

## Libraries
- **Tests:** Use NUnit and NSubstitute
