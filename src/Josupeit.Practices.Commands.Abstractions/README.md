# Josupeit.Practices.Commands.Abstractions

When you apply the CQRS pattern you quickly end up repeating the same small set of method signatures — `Execute`, `ExecuteAsync`, `Execute(input)`, and so on — across dozens of command and query types. This package captures those contracts as a family of interfaces so that every command or query in your application speaks the same language, regardless of where it comes from or how it is implemented.

A deliberate design choice here is that every interface exposes **both a synchronous and an asynchronous execution path**. That way a caller never has to care whether the concrete implementation is sync or async underneath.

```
dotnet add package Josupeit.Practices.Commands.Abstractions
```

All types live in the `Josupeit.Practices.Commands` namespace; the package name's `.Abstractions` suffix is not part of it.

## Interfaces

The interfaces are organized along two axes: whether the operation takes an input parameter, and whether it returns a result.

| Interface                             | Input    | Output    | Notes                                                           |
| ------------------------------------- | -------- | --------- | --------------------------------------------------------------- |
| `ICommand`                            | —        | —         | Side-effecting, no return value                                 |
| `ICommand<TInput>`                    | `TInput` | —         |                                                                 |
| `ICommandWithOutput<TOutput>`         | —        | `TOutput` |                                                                 |
| `ICommandWithOutput<TInput, TOutput>` | `TInput` | `TOutput` |                                                                 |
| `IQuery<TOutput>`                     | —        | `TOutput` | Pure — implementations must not produce observable side effects |
| `IQuery<TInput, TOutput>`             | `TInput` | `TOutput` | Pure                                                            |

The distinction between `ICommandWithOutput` and `IQuery` is intentional. A command with output is still allowed to modify state; a query is not. The `IQuery` methods are annotated with `[Pure]` to make this contract explicit.

Every interface provides three members, one synchronous and two asynchronous:

```csharp
void Execute();                                    // synchronous
Task ExecuteAsync();                               // asynchronous, no cancellation
Task ExecuteAsync(CancellationToken cancellationToken); // asynchronous, with cancellation
```

## When to use this package directly

Reference `Josupeit.Practices.Commands.Abstractions` in libraries or shared application layers that need to work with commands and queries without depending on a specific implementation. For example, a pipeline or mediator component that dispatches commands only needs to know about `ICommand<TInput>`, not about any concrete base class.

```csharp
public interface ICommandDispatcher
{
    Task DispatchAsync<TInput>(ICommand<TInput> command, TInput input, CancellationToken cancellationToken);
}
```

For the concrete base classes that implement these interfaces — and that bridge the sync and async paths automatically — install `Josupeit.Practices.Commands` instead.
