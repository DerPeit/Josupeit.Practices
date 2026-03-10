# Josupeit.Practices.Specifications

Business rules rarely live in isolation. A "premium active user" is an active user who is also a premium subscriber. An "eligible order" might be one that is paid, not yet shipped, and placed within the last 30 days. When you hard-code these combinations as ad hoc boolean expressions scattered through your code, they become hard to name, test, and reuse.

The specification pattern addresses this by encapsulating each individual rule as a named, reusable object. This package provides a `Specification<TCandidate>` base class with C# operator overloads so that composing rules reads almost like plain English.

Override either `IsSatisfiedBy` or `IsSatisfiedByAsync`, and the other direction is bridged automatically.

```
dotnet add package Josupeit.Practices.Specifications
```

## How to implement a specification

```csharp
public sealed class ActiveUserSpec : Specification<User>
{
    // Sync implementation — the async overload is bridged automatically.
    public override bool IsSatisfiedBy(User user) => user.IsActive;
}

public sealed class PremiumUserSpec : Specification<User>
{
    public override bool IsSatisfiedBy(User user) => user.Tier == Tier.Premium;
}

// When your rule requires I/O, override the async path instead.
public sealed class HasUnpaidInvoiceSpec : Specification<User>
{
    private readonly IInvoiceRepository _invoices;

    public HasUnpaidInvoiceSpec(IInvoiceRepository invoices) => _invoices = invoices;

    public override async Task<bool> IsSatisfiedByAsync(User user, CancellationToken cancellationToken)
    {
        return await _invoices.AnyUnpaidAsync(user.Id, cancellationToken);
    }
}
```

## Composing specifications

The real power of the pattern comes from composition. Use the standard C# logical operators to build compound specifications from simpler ones. The result is itself a `Specification<TCandidate>` and can be composed further.

```csharp
var eligibleForPromotion = new ActiveUserSpec() && new PremiumUserSpec() && !new HasUnpaidInvoiceSpec(invoices);

// Evaluate synchronously or asynchronously — your choice.
bool isSatisfied      = eligibleForPromotion.IsSatisfiedBy(user);
bool isSatisfiedAsync = await eligibleForPromotion.IsSatisfiedByAsync(user, cancellationToken);
```

## Operator reference

| Operator | Semantics |
|---|---|
| `a && b` | Both specifications must be satisfied (AND) |
| `a || b` | At least one specification must be satisfied (OR) |
| `a ^ b` | Exactly one specification must be satisfied (XOR) |
| `!a` | The specification must not be satisfied (NOT) |
