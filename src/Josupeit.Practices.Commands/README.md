# Josupeit.Practices.Commands

Implementing both `Execute` and `ExecuteAsync` on every command is tedious and error-prone. If the two implementations diverge, callers that happen to use the synchronous path silently get different behavior. This package solves that by providing abstract base classes where you only implement **one** variant — sync or async — and the other is derived automatically.

The bridging works in both directions. If you override `ExecuteAsync`, then `Execute` will block on it. If you override `Execute`, then `ExecuteAsync` will wrap it. If you accidentally call the base implementation from an override without actually implementing the other direction, the base class detects the recursion and throws an `InvalidOperationException` with a clear message rather than causing a stack overflow.

```
dotnet add package Josupeit.Practices.Commands
```

All types live in the `Josupeit.Practices.Commands` namespace, shared with the Abstractions package.

## How to implement a command

Override **either** `Execute` (sync) **or** `ExecuteAsync(CancellationToken)` (async), but not both unless your implementation genuinely differs between the two. Never call `base.Execute` or `base.ExecuteAsync` from an override.

### Parameterless command (no return value)

```csharp
public sealed class SendWelcomeEmailCommand : Command
{
    private readonly IEmailService _email;

    public SendWelcomeEmailCommand(IEmailService email) => _email = email;

    // Override the async path. Execute() is bridged automatically.
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _email.SendAsync("welcome@example.com", cancellationToken).ConfigureAwait(false);
    }
}

// Both call sites work without any extra effort.
new SendWelcomeEmailCommand(email).Execute();
await new SendWelcomeEmailCommand(email).ExecuteAsync(cancellationToken);
```

### Command with input

```csharp
public sealed class DeleteUserCommand : Command<Guid>
{
    private readonly IUserRepository _repository;

    public DeleteUserCommand(IUserRepository repository) => _repository = repository;

    public override async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
```

### Command with output

```csharp
public sealed class CreateOrderCommand : CommandWithOutput<Order>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommand(IOrderRepository repository) => _repository = repository;

    public override async Task<Order> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _repository.CreateAsync(cancellationToken).ConfigureAwait(false);
    }
}
```

### Query (pure, no side effects)

Queries are expected to be pure. Implement the synchronous path when the implementation does not involve I/O, and the base class will handle the async path.

```csharp
public sealed class GetUserQuery : Query<Guid, User>
{
    private readonly IUserRepository _repository;

    public GetUserQuery(IUserRepository repository) => _repository = repository;

    public override User Execute(Guid id) => _repository.Find(id);
}
```

## Available base classes

| Base class                           | Input    | Output    |
| ------------------------------------ | -------- | --------- |
| `Command`                            | —        | —         |
| `Command<TInput>`                    | `TInput` | —         |
| `CommandWithOutput<TOutput>`         | —        | `TOutput` |
| `CommandWithOutput<TInput, TOutput>` | `TInput` | `TOutput` |
| `Query<TOutput>`                     | —        | `TOutput` |
| `Query<TInput, TOutput>`             | `TInput` | `TOutput` |

The interfaces that these classes implement live in `Josupeit.Practices.Commands.Abstractions`, which is pulled in automatically as a transitive dependency.
